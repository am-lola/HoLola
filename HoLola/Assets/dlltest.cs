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

    public void makebreakvl()
    {
#if UNITY_EDITOR
        throw new System.NotImplementedException("This method cannot be called from the editor");
#else
        Debug.Log("Initializing comms");
        bool success = LolaComms.SampleClass.init();
        if (!success)
        {
            Debug.Log("OMFG WHAT NOW");
            return;
        }

        Debug.Log("Setting info callback...");
        LolaComms.SampleClass.RegisterInfoCallback((str) => Debug.Log("INFO: " + str));
        Debug.Log("Oh, it worked?");
        IntPtr vl = LolaComms.SampleClass.VisionListener_Create(9090);
        LolaComms.SampleClass.VisionListener_OnError(vl, (errstr) => Debug.Log(errstr));
        LolaComms.SampleClass.VisionListener_OnConnect(vl, (host) => Debug.Log("Connected to: " + host));
        LolaComms.SampleClass.VisionListener_OnDisconnect(vl, (host) => Debug.Log("Disconnected from: " + host));
        LolaComms.SampleClass.VisionListener_OnObstacleMessage(vl, (msg) => {
            Debug.Log("New obstacle: " + msg.ToString());
            //Marshal.FreeHGlobal(msg);
            }
        );
        LolaComms.SampleClass.VisionListener_Listen(vl);
        Debug.Log("Listener is listening: " + LolaComms.SampleClass.VisionListener_IsListening(vl));
#endif
    }
    // Use this for initialization
    void Start () {
        Debug.Log("Starting up! :D");

        makebreakvl();
	}
	
}
