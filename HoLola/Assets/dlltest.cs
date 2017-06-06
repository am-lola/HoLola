using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine;

/// <summary>
/// Just a quick & dirty test to verify PInvokes are working correctly.
/// </summary>

public class dlltest : MonoBehaviour {
    
    public string doSample(float f)
    {
#if UNITY_EDITOR
        throw new System.NotImplementedException("This method cannot be called from the editor");
#else
        return LolaComms.SampleClass.SampleFunc(f);
#endif
    }
    // Use this for initialization
    void Start () {
        Debug.Log("Starting up! :D");
        Debug.Log("Sample (5): " + doSample(5));
	}
	
}
