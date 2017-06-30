using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System;
using UnityEngine;

/// <summary>
/// Just a quick & dirty test to verify PInvokes are working correctly.
/// </summary>

public class dlltest : MonoBehaviour {
    public Material obstacleMaterial;
    public GameObject Capsule;
    public GameObject Plane;
#if UNITY_EDITOR
#else
    private LolaComms.VisionListener vl;
    private Dictionary<uint, GameObject> obstacle_map = new Dictionary<uint, GameObject>();
    private Dictionary<uint, GameObject> surface_map  = new Dictionary<uint, GameObject>();

    public LolaComms.VisionListener SetupVL()
    {
        Debug.Log("Setting info callback...");
        LolaComms.Common.RegisterInfoCallback((str) => Debug.Log("INFO: " + str));
        Debug.Log("Initializing comms");

        bool success = LolaComms.Common.Init();
        if (!success)
        {
            Debug.Log("Failed to initialize networking!");
            return null;
        }

        LolaComms.VisionListener vl = new LolaComms.VisionListener(9090);
        vl.onError += (errstr) => Debug.Log(errstr);
        vl.onConnect += (host) => Debug.Log("Connected to: " + host);
        vl.onDisconnect += (host) => Debug.Log("Disconnected from: " + host);
        vl.onObstacleMessage += (msg) =>
        {
            Debug.Log("New obstacle message: " + msg.ToString());

            switch (msg.action)
            {
                case (uint)LolaComms.Common.MsgId.SET_SSV:
                {
                    if (msg.type == LolaComms.ObstacleType.Sphere)
                    {
                        GameObject sphere;
                        if (obstacle_map.ContainsKey(msg.model_id))
                        {
                            sphere = obstacle_map[msg.model_id];
                        }
                        else
                        {
                            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        }
                        sphere.transform.parent = transform;
                        sphere.transform.localPosition = new Vector3(msg.coeffs[0], msg.coeffs[1], msg.coeffs[2]);
                        sphere.transform.localScale = new Vector3(msg.radius, msg.radius, msg.radius);
                        sphere.GetComponent<MeshRenderer>().material = obstacleMaterial;
                        obstacle_map.Add(msg.model_id, sphere);
                    }
                    else if (msg.type == LolaComms.ObstacleType.Capsule)
                    {
                        GameObject cap;
                        if (obstacle_map.ContainsKey(msg.model_id))
                        {
                            cap = obstacle_map[msg.model_id];
                        }
                        else
                        {
                            cap = Instantiate(Capsule, this.transform);
                        }
                        Capsule c = cap.GetComponent<Capsule>();
                        c.Radius = msg.radius;
                        c.Top = new Vector3(msg.coeffs[0], msg.coeffs[1], msg.coeffs[2]);
                        c.Bottom = new Vector3(msg.coeffs[3], msg.coeffs[4], msg.coeffs[5]);

                        obstacle_map.Add(msg.model_id, cap);
                    }
                    break;
                }
                case (uint)LolaComms.Common.MsgId.MODIFY_SSV:
                {
                    if (obstacle_map.ContainsKey(msg.model_id))
                    {
                        if (msg.type == LolaComms.ObstacleType.Sphere)
                        {
                            obstacle_map[msg.model_id].transform.localPosition = new Vector3(msg.coeffs[0], msg.coeffs[1], msg.coeffs[2]);
                            obstacle_map[msg.model_id].transform.localScale = new Vector3(msg.radius, msg.radius, msg.radius) * 5.0f;
                        }
                        else if (msg.type == LolaComms.ObstacleType.Capsule)
                        {
                            Capsule c = obstacle_map[msg.model_id].GetComponent<Capsule>();
                            c.Radius = msg.radius;
                            c.Top = new Vector3(msg.coeffs[0], msg.coeffs[1], msg.coeffs[2]);
                            c.Bottom = new Vector3(msg.coeffs[3], msg.coeffs[4], msg.coeffs[5]);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Got MODIFY_SSV msg for non-existent object: " + msg.model_id);
                    }
                    break;
                }
                case (uint)LolaComms.Common.MsgId.REMOVE_SSV_ONLY_PART:
                {
                    if (obstacle_map.ContainsKey(msg.model_id))
                    {
                        Destroy(obstacle_map[msg.model_id]);
                        obstacle_map.Remove(msg.model_id);
                    }
                    else
                    {
                        Debug.LogWarning("Got REMOVE_SSV_ONLY_PART msg for non-existen object: " + msg.model_id);
                    }
                    break;
                }
                case (uint)LolaComms.Common.MsgId.REMOVE_SSV_WHOLE_SEGMENT:
                {
                    if (obstacle_map.ContainsKey(msg.model_id))
                    {
                        Destroy(obstacle_map[msg.model_id]);
                        obstacle_map.Remove(msg.model_id);
                    }
                    else
                    {
                        Debug.LogWarning("Got REMOVE_SSV_WHOLE_SEGMENT msg for non-existen object: " + msg.model_id);
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
        Debug.Log("Listener is listening: " + vl.Listening);
        return vl;

    }
    // Use this for initialization
    void Start () {
        Debug.Log("Starting up! :D");

        vl = SetupVL();
	}

    private void Update()
    {
        vl.Process();
    }
#endif

}
