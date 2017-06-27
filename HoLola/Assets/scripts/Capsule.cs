using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Capsule : MonoBehaviour {
    Transform cap1; // capsule end-caps
    Transform cap2;

	// Use this for initialization
	void Start () {
        
        cap1 = FindInChildren(this.transform, "cap_1");
        cap2 = FindInChildren(this.transform, "cap_2");
	}

    Transform FindInChildren(Transform parent, string name)
    {
        var result = parent.Find(name);
        if (result != null)
            return result;
        foreach (Transform child in parent)
        {
            result = FindInChildren(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    // Update is called once per frame
    void Update () {
        cap1.LookAt(cap2, Vector3.up);
        cap2.LookAt(cap1, Vector3.down);
    }
}
