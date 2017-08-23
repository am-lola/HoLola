using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System;
using UnityEngine;

/// <summary>
/// Just a quick & dirty test to verify PInvokes are working correctly.
/// </summary>

public class RobotData : MonoBehaviour {
    public Material obstacleMaterial;
    public GameObject Capsule;
    public GameObject Plane;
    public GameObject Footprint;
    public Material foot_left;
    public Material foot_right;
    [Space]
    public int VisionPort    = 9090;
    public int FootstepPort  = 61448;
    public string FootstepIP = "192.168.0.7";
    public int PosePort      = 53249;
    [Space]
    public UnityEngine.UI.InputField VisionPortField;
    public UnityEngine.UI.InputField FootstepPortField;
    public UnityEngine.UI.InputField PosePortField;
    [Space]
    public StatusButton VisionStatus;
    public StatusButton FootstepStatus;
    public StatusButton PoseStatus;

    public void OnVisionPortChanged()
    {
        VisionPort = Int32.Parse(VisionPortField.text);
    }
    public void OnFootstepPortChanged()
    {
        FootstepPort = Int32.Parse(FootstepPortField.text);
    }
    public void OnPosePortChanged()
    {
        PosePort = Int32.Parse(PosePortField.text);
    }

#if UNITY_EDITOR
#else
    private LolaComms.VisionListener vl;
    private LolaComms.PoseListener pl;
    private LolaComms.FootstepListener fl;
    private Dictionary<string, GameObject> obstacle_map = new Dictionary<string, GameObject>();
    private Dictionary<uint, GameObject> surface_map  = new Dictionary<uint, GameObject>();

    struct FootData
    {
        public GameObject go;
        public LolaComms.Footstep step;
        public FootData(GameObject g, LolaComms.Footstep s)
        {
            go = g;
            step = s;
        }
    }

    int max_steps = 8; // max # of footsteps to accept for a given foot in one time step
    private List<FootData> footsteps_L = new List<FootData>(); // footsteps planned for current time step
    private List<FootData> footsteps_L_real = new List<FootData>(); // previously-taken actual footsteps
    private float left_foot_angle = 0;
    private float left_foot_angle_real = 0;
    private List<FootData> footsteps_R = new List<FootData>();
    private List<FootData> footsteps_R_real = new List<FootData>();
    private float right_foot_angle = 0; // current angle of last received footstep
    private float right_foot_angle_real = 0; // current angle of last *actually-taken* footstep

    private bool InitComms()
    {
        Debug.Log("Setting LolaComms info callback...");
        LolaComms.Common.RegisterInfoCallback((str) => Debug.Log("INFO: " + str));

        Debug.Log("Initializing comms");
        bool success = LolaComms.Common.Initialize();
        if (!success)
        {
            Debug.LogError("Failed to initialize networking!");
        }

        return success;
    }

    private void AddFootstep(List<FootData> collection, LolaComms.Footstep step)
    {
        var stepobj = Instantiate(Footprint, transform);
        stepobj.transform.localPosition = new Vector3(step.start_x, step.start_y, step.start_z);

        var foot = (LolaComms.Foot)step.stance;
        switch (foot)
        {
            case LolaComms.Foot.Left:
            {
                stepobj.transform.localRotation = Quaternion.AngleAxis(left_foot_angle, new Vector3(0, 0, 1)) * stepobj.transform.localRotation;
                stepobj.GetComponentInChildren<MeshRenderer>().material = foot_left;
            }
            break;
            case LolaComms.Foot.Right:
            {
                stepobj.transform.localRotation = Quaternion.AngleAxis(right_foot_angle, new Vector3(0, 0, 1)) * stepobj.transform.localRotation;
                stepobj.GetComponentInChildren<MeshRenderer>().material = foot_right;
            }
            break;
            default:
            {
                Debug.LogError("Unknown foot stance: " + step.stance);
            }
            break;
        }

        // generate path between current and previous step
        if (collection.Count > 0)
        {
            var line = stepobj.GetComponent<LineRenderer>();
            var start_pos = collection[collection.Count - 1].go.transform.position;
            var end_pos = stepobj.transform.position;
            var mid_pos = start_pos + new Vector3(start_pos.x + (end_pos.x - start_pos.x) / 2,
                                                  start_pos.y + step.dy,
                                                  start_pos.z + step.dz_step);
            line.positionCount = 10;
            for (int i = 0; i < line.positionCount/2; i++)
            {
                line.SetPosition(i, Vector3.Slerp(start_pos, mid_pos, (float)i / (float)(line.positionCount / 2)));
            }
            for (int i = line.positionCount/2; i < line.positionCount; i++)
            {
                line.SetPosition(i, Vector3.Slerp(mid_pos, end_pos, (float)i / (float)(line.positionCount / 2)));
            }
            line.positionCount = line.positionCount + 1;
            line.SetPosition(line.positionCount - 1, end_pos);
        }

        collection.Add(new FootData(stepobj, step));
    }

    private LolaComms.FootstepListener SetupFL()
    {
        Debug.Log("Setting up FootstepListener...");
        LolaComms.FootstepListener fl = new LolaComms.FootstepListener(FootstepPort, "192.168.0.7");
        fl.onError += (errstr) =>
        {
            Debug.LogError("[Footsteps] " + errstr);
        };

        fl.onConnect += (host) =>
        {
            Debug.Log("[Footsteps] Connected to: " + host);
        };

        fl.onDisonnect += (host) =>
        {
            Debug.Log("[Footsteps] Disconnected from: " + host);
        };

        fl.onNewStep += (step) =>
        {
            Debug.Log("Got new footstep for " + (LolaComms.Foot)(step.stance) + "{" + step.stamp_gen + "} @ [" + step.start_x + ", " + step.start_y + ", " + step.start_z + "], start_phi_z: " + step.start_phi_z +
                ", phi0: " + step.phiO);

            var foot = (LolaComms.Foot)step.stance;
            switch (foot)
            {
                case LolaComms.Foot.Left:
                {
                    if (footsteps_L.Count == 0 ||
                       (footsteps_L.Count > 0 && footsteps_L.Count < max_steps &&
                        footsteps_L[0].step.stamp_gen == step.stamp_gen))
                    {
                        left_foot_angle += step.phi_leg_rel;
                        AddFootstep(footsteps_L, step);
                    }
                    else // new timestamp
                    {
                        footsteps_L_real.Add(footsteps_L[0]); // first footstep from each stamp is robot's actual standing pose
                        left_foot_angle_real += footsteps_L[0].step.phi_leg_rel;
                        left_foot_angle = left_foot_angle_real;

                        // delete all other steps
                        footsteps_L.RemoveAt(0);
                        foreach (var footstep in footsteps_L)
                        {
                            Destroy(footstep.go);
                        }
                        footsteps_L.Clear();

                        // add new footstep as normal
                        left_foot_angle += step.phi_leg_rel;
                        AddFootstep(footsteps_L, step);
                    }
                }
                break;
                case LolaComms.Foot.Right:
                {
                    if (footsteps_R.Count == 0 ||
                       (footsteps_R.Count > 0 && footsteps_R.Count < max_steps &&
                        footsteps_R[0].step.stamp_gen == step.stamp_gen))
                    {
                        right_foot_angle += step.phi_leg_rel;
                        AddFootstep(footsteps_R, step);
                    }
                    else // new timestamp
                    {
                        footsteps_R_real.Add(footsteps_R[0]); // first footstep from each stamp is robot's actual standing pose
                        right_foot_angle_real += footsteps_R[0].step.phi_leg_rel;
                        right_foot_angle = right_foot_angle_real;

                        // delete all other steps
                        footsteps_R.RemoveAt(0);
                        foreach (var footstep in footsteps_R)
                        {
                            Destroy(footstep.go);
                        }
                        footsteps_R.Clear();

                        // add new footstep as normal
                        right_foot_angle += step.phi_leg_rel;
                        AddFootstep(footsteps_R, step);
                    }
                }
                break;
                default:
                {
                    Debug.LogError("Unknown foot stance value: " + step.stance);
                }
                break;
            }

        };
        fl.Listen();

        Debug.Log("FootstepListener: " + (fl.Listening ? "is listening" : "is NOT listening"));
        return fl;
    }

    private LolaComms.PoseListener SetupPL()
    {
        Debug.Log("Setting up PoseListener...");
        LolaComms.PoseListener pl = new LolaComms.PoseListener(PosePort);
        pl.onError += (errstr) =>
        {
            Debug.LogError("[Pose] " + errstr);
        };
        pl.onNewPose += (pose) => Debug.Log("[Pose] New pose: S: " + pose.stamp + " T: " + pose.t_wr_cl + ", R: " + pose.R_wr_cl);
        pl.Listen();

        Debug.Log("PoseListener: " + (pl.Listening ? "is listening" : "is NOT listening"));

        return pl;
    }

    private LolaComms.VisionListener SetupVL()
    {
        Debug.Log("Setting up VisionListener...");
        LolaComms.VisionListener vl = new LolaComms.VisionListener(VisionPort);
        vl.onError += (errstr) =>
        {
            Debug.Log(errstr);
        };
        vl.onConnect += (host) =>
        {
            Debug.Log("Connected to: " + host);
        };

        vl.onDisconnect += (host) =>
        {
            Debug.Log("Disconnected from: " + host);
        };

        vl.onObstacleMessage += (msg) =>
        {
            Debug.Log("New obstacle message: " + msg.ToString());

            switch (msg.action)
            {
                case (uint)LolaComms.Common.MsgId.SET_SSV:
                {
                    if (msg.type == LolaComms.ObstacleType.Sphere)
                    {
                        if (obstacle_map.ContainsKey(msg.IdString()))
                        {
                            Debug.LogWarning("Received SET_SSV command for existing objec: " + msg.IdString());
                            Destroy(obstacle_map[msg.IdString()]);
                            obstacle_map.Remove(msg.IdString());
                        }

                        obstacle_map.Add(msg.IdString(), CreateSphere(msg.radius, new Vector3(msg.coeffs[0], msg.coeffs[1], msg.coeffs[2]), transform));
                    }
                    else if (msg.type == LolaComms.ObstacleType.Capsule)
                    {
                        if (obstacle_map.ContainsKey(msg.IdString()))
                        {
                            Debug.LogWarning("Received SET_SSV command for existing objec: " + msg.IdString());
                            Destroy(obstacle_map[msg.IdString()]);
                            obstacle_map.Remove(msg.IdString());
                        }

                        obstacle_map.Add(msg.IdString(),
                            CreateCapsule(msg.radius,
                              new Vector3(msg.coeffs[0], msg.coeffs[1], msg.coeffs[2]),
                              new Vector3(msg.coeffs[3], msg.coeffs[4], msg.coeffs[5]),
                              transform)
                        );
                    }
                    break;
                }
                case (uint)LolaComms.Common.MsgId.MODIFY_SSV:
                {
                    if (obstacle_map.ContainsKey(msg.IdString()))
                    {
                        // in case we get a sphere replacing a capsule or vice versa, just replace what we had
                        Destroy(obstacle_map[msg.IdString()]);
                        if (msg.type == LolaComms.ObstacleType.Sphere)
                        {
                            obstacle_map[msg.IdString()] = CreateSphere(msg.radius, new Vector3(msg.coeffs[0], msg.coeffs[1], msg.coeffs[2]), transform);
                        }
                        else if (msg.type == LolaComms.ObstacleType.Capsule)
                        {
                            obstacle_map[msg.IdString()] = CreateCapsule(msg.radius,
                                                            new Vector3(msg.coeffs[0], msg.coeffs[1], msg.coeffs[2]),
                                                            new Vector3(msg.coeffs[3], msg.coeffs[4], msg.coeffs[5]),
                                                            transform);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Got MODIFY_SSV msg for non-existent object: " + msg.IdString());
                        if (msg.type == LolaComms.ObstacleType.Sphere)
                        {
                            obstacle_map.Add(msg.IdString(), CreateSphere(msg.radius, new Vector3(msg.coeffs[0], msg.coeffs[1], msg.coeffs[2]), transform));
                        }
                        else if (msg.type == LolaComms.ObstacleType.Capsule)
                        {
                            obstacle_map.Add(msg.IdString(),
                                CreateCapsule(msg.radius,
                                  new Vector3(msg.coeffs[0], msg.coeffs[1], msg.coeffs[2]),
                                  new Vector3(msg.coeffs[3], msg.coeffs[4], msg.coeffs[5]),
                                  transform)
                            );
                        }
                    }
                    break;
                }
                case (uint)LolaComms.Common.MsgId.REMOVE_SSV_ONLY_PART:
                {
                    if (obstacle_map.ContainsKey(msg.IdString()))
                    {
                        Destroy(obstacle_map[msg.IdString()]);
                        obstacle_map.Remove(msg.IdString());
                    }
                    else
                    {
                        Debug.LogWarning("Got REMOVE_SSV_ONLY_PART msg for non-existent object: " + msg.model_id);
                    }
                    break;
                }
                case (uint)LolaComms.Common.MsgId.REMOVE_SSV_WHOLE_SEGMENT:
                {
                    List<string> to_remove = new List<string>();
                    foreach (var obstacle in obstacle_map) /// TODO: Evaluate whether we should use a different structure to optimize this case
                    {
                        if (obstacle.Key.Split('|')[0] == msg.model_id.ToString())
                        {
                            Destroy(obstacle.Value);
                            to_remove.Add(obstacle.Key);
                        }
                    }

                    if (to_remove.Count == 0)
                    {
                        Debug.LogWarning("Got REMOVE_SSV_WHOLE_SEGMENT msg for non-existent object: " + msg.model_id);
                    }
                    else
                    {
                        foreach (var item in to_remove)
                        {
                            obstacle_map.Remove(item);
                        }
                    }
                    break;
                }
                default:
                    Debug.LogWarning("Got unexpected SSV action: " + msg.action);
                    break;
            }
        };
        vl.onSurfaceMessage += (msg) =>
        {
            Debug.Log("Got a new surface: " + msg.ToString());

            switch (msg.action)
            {
                case (uint)LolaComms.Common.MsgId.SET_SURFACE: // add new surface
                {
                    GameObject new_surface;
                    if (!surface_map.ContainsKey(msg.id)) // only create new object if this surface is really new
                    {
                        new_surface = Instantiate(Plane, this.transform);
                    }
                    else // if we have this surface already, replace the existing mesh data
                    {
                        new_surface = surface_map[msg.id];
                    }
                    var mesh = new_surface.GetComponent<MeshFilter>();
                    mesh.mesh = new Mesh();

                    // set vertices
                    var verts = new Vector3[msg.vertices.Length / 3];
                    for (int i = 0; i < verts.Length; i++)
                    {
                        verts[i] = new Vector3(msg.vertices[i*3], msg.vertices[i*3 + 1], msg.vertices[i*3 + 2]);
                    }
                    mesh.mesh.vertices = verts;

                    // make triangle fan from vertices
                    var tris = new int[18]; // 6 triangles w/ 3 verts each
                    tris[0] = 0;
                    tris[1] = 1;
                    tris[2] = 2;

                    tris[3] = 0;
                    tris[4] = 2;
                    tris[5] = 3;

                    tris[6] = 0;
                    tris[7] = 3;
                    tris[8] = 4;

                    tris[9]  = 0;
                    tris[10] = 4;
                    tris[11] = 5;

                    tris[12] = 0;
                    tris[13] = 5;
                    tris[14] = 6;

                    tris[15] = 0;
                    tris[16] = 6;
                    tris[17] = 7;

                    mesh.mesh.triangles = tris;

                    // set normals
                    var normals = new Vector3[verts.Length];
                    for (int i = 0; i < normals.Length; i++)
                    {
                        normals[i] = new Vector3(msg.normal[0], msg.normal[1], msg.normal[2]);
                    }
                    mesh.mesh.normals = normals;

                    if (surface_map.ContainsKey(msg.id)) // don't add surface if it already exists
                    {
                        Debug.LogWarning("Received SET_SURFACE msg for already existing surface ID: " + msg.id);
                    }
                    else
                    {
                        surface_map.Add(msg.id, new_surface);
                    }

                    mesh.mesh.RecalculateBounds();
                    break;
                }
                case (uint)LolaComms.Common.MsgId.MODIFY_SURFACE:
                {
                    if (!surface_map.ContainsKey(msg.id))
                    {
                        Debug.LogError("Received MODIFY_SURFACE msg for non-existent surface ID: " + msg.id);
                        break;
                    }
                    var surface = surface_map[msg.id];
                    var mesh = surface.GetComponent<MeshFilter>();
                    mesh.mesh = new Mesh();

                    // set vertices
                    var verts = new Vector3[msg.vertices.Length / 3];
                    for (int i = 0; i < verts.Length; i++)
                    {
                        verts[i] = new Vector3(msg.vertices[i * 3], msg.vertices[i * 3 + 1], msg.vertices[i * 3 + 2]);
                    }
                    mesh.mesh.vertices = verts;

                    // make triangle fan from vertices
                    var tris = new int[18]; // 6 triangles w/ 3 verts each
                    tris[0] = 0;
                    tris[1] = 1;
                    tris[2] = 2;

                    tris[3] = 0;
                    tris[4] = 2;
                    tris[5] = 3;

                    tris[6] = 0;
                    tris[7] = 3;
                    tris[8] = 4;

                    tris[9] = 0;
                    tris[10] = 4;
                    tris[11] = 5;

                    tris[12] = 0;
                    tris[13] = 5;
                    tris[14] = 6;

                    tris[15] = 0;
                    tris[16] = 6;
                    tris[17] = 7;

                    mesh.mesh.triangles = tris;

                    // set normals
                    var normals = new Vector3[verts.Length];
                    for (int i = 0; i < normals.Length; i++)
                    {
                        normals[i] = new Vector3(msg.normal[0], msg.normal[1], msg.normal[2]);
                    }
                    mesh.mesh.normals = normals;

                    mesh.mesh.RecalculateBounds();
                    break;
                }
                case (uint)LolaComms.Common.MsgId.REMOVE_SURFACE:
                {
                    if (surface_map.ContainsKey(msg.id))
                    {
                        Destroy(surface_map[msg.id]);
                        surface_map.Remove(msg.id);
                    }
                    else
                    {
                        Debug.LogWarning("Got REMOVE_SURFACE msg for non-existent surface: " + msg.id);
                    }
                    break;
                }
                case (uint)LolaComms.Common.MsgId.RESET_SURFACEMAP: // clear ALL surfaces
                {
                    foreach (var surface in surface_map)
                    {
                        Destroy(surface.Value);
                    }
                    surface_map.Clear();
                    break;
                }
                default:
                Debug.LogError("Got unknown surface action: " + msg.action);
                break;
            }

        };

        vl.Listen();
        Debug.Log("VisionListener is listening: " + vl.Listening);
        return vl;

    }

    GameObject CreateSphere(float radius, Vector3 position, Transform parent)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.parent = parent;
        sphere.transform.localPosition = position;
        sphere.transform.localScale = new Vector3(radius, radius, radius) * 2.0f; // unity default spheres have radius 1.0
        sphere.GetComponent<MeshRenderer>().material = obstacleMaterial;

        return sphere;
    }

    GameObject CreateCapsule(float radius, Vector3 pos1, Vector3 pos2, Transform parent)
    {
        GameObject cap = Instantiate(Capsule, parent);
        Capsule c = cap.GetComponent<Capsule>();
        c.Radius = radius;
        c.Top = pos1;
        c.Bottom = pos2;

        return cap;
    }

    public void RestartVisionListener()
    {
        if (vl.Listening)
        {
            vl.Stop();
        }
        VisionStatus.status = StatusButton.Status.Neutral;
        vl = SetupVL();
    }
    public void RestartFootstepListener()
    {
        if (fl.Listening)
        {
            fl.Stop();
        }
        FootstepStatus.status = StatusButton.Status.Neutral;
        fl = SetupFL();
    }
    public void RestartPoseListener()
    {
        if (pl.Listening)
        {
            pl.Stop();
        }
        PoseStatus.status = StatusButton.Status.Neutral;
        pl = SetupPL();
    }

    void Start () {
        Debug.Log("Starting up! :D");

        VisionPortField.text = VisionPort.ToString();
        FootstepPortField.text = FootstepPort.ToString();
        PosePortField.text = PosePort.ToString();

        if (InitComms())
        {
            vl = SetupVL();
            pl = SetupPL();
            fl = SetupFL();
        }
	}

    private void OnDestroy()
    {
        if (LolaComms.Common.Initialized())
        {
            LolaComms.Common.Uninitialize();
        }
    }

    private void Update()
    {
        if (vl.Listening)
        {
            VisionStatus.status = StatusButton.Status.Good;
            vl.Process();
        }
        else
        {
            VisionStatus.status = StatusButton.Status.Bad;
        }

        if (fl.Listening)
        {
            FootstepStatus.status = StatusButton.Status.Good;
            fl.Process();
        }
        else
        {
            FootstepStatus.status = StatusButton.Status.Bad;
        }

        if (pl.Listening)
        {
            PoseStatus.status = StatusButton.Status.Good;
            pl.Process();
        }
        else
        {
            PoseStatus.status = StatusButton.Status.Bad;
        }

    }
#endif

}
