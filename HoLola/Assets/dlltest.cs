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
#if UNITY_EDITOR
#else
    private LolaComms.VisionListener vl;
    private Dictionary<uint, GameObject> obstacle_map = new Dictionary<uint, GameObject>();

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
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        sphere.transform.parent = transform;
                        sphere.transform.localPosition = new Vector3(msg.coeffs[0], msg.coeffs[1], msg.coeffs[2]);
                        sphere.transform.localScale = new Vector3(msg.radius, msg.radius, msg.radius);
                        sphere.GetComponent<MeshRenderer>().material = obstacleMaterial;
                        obstacle_map.Add(msg.model_id, sphere);
                    }
                    else if (msg.type == LolaComms.ObstacleType.Capsule)
                    {
                        GameObject cap = Instantiate(Capsule, this.transform);
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
                    break;
                }
                case (uint)LolaComms.Common.MsgId.REMOVE_SSV_ONLY_PART:
                {
                    if (obstacle_map.ContainsKey(msg.model_id))
                    {
                        Destroy(obstacle_map[msg.model_id]);
                        obstacle_map.Remove(msg.model_id);
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
                    break;
                }
                default:
                Debug.LogWarning("Got unexpected SSV action: " + msg.action);
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
