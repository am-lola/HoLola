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

#if UNITY_EDITOR
#else
    public LolaComms.VisionListener makebreakvl()
    {
        Debug.Log("Initializing comms");

        bool success = LolaComms.Common.Init();
        if (!success)
        {
            Debug.Log("Failed to initialize networking!");
            return null;
        }

        Debug.Log("Setting info callback...");
        LolaComms.Common.RegisterInfoCallback((str) => Debug.Log("INFO: " + str));

        LolaComms.VisionListener vl = new LolaComms.VisionListener(9090);
        vl.onError += (errstr) => Debug.Log(errstr);
        vl.onConnect += (host) => Debug.Log("Connected to: " + host);
        vl.onDisconnect += (host) => Debug.Log("Disconnected from: " + host);
        vl.onObstacleMessage += (msg) => Debug.Log("New obstacle: " + msg.ToString());
        vl.Listen();
        Debug.Log("Listener is listening: " + vl.Listening);
        return vl;

    }
    // Use this for initialization
    void Start () {
        Debug.Log("Starting up! :D");

        LolaComms.VisionListener vl = makebreakvl();
	}
#endif
	
}
