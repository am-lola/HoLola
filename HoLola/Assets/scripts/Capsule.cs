using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Capsule : MonoBehaviour {
    Transform cap1; // capsule end-caps
    Transform cap2;

    public Vector3 Top
    {
        get { return cap1.position; }
        set
        {
            cap1.localPosition = value;
            Reorient();
        }
    }
    public Vector3 Bottom
    {
        get { return cap2.position; }
        set
        {
            cap2.localPosition = value;
            Reorient();
        }
    }

    public float Radius
    {
        get { return cap1.localScale.x; } // all scales should be uniform
        set
        {
            cap1.localScale = new Vector3(value, value, value);
            cap2.localScale = new Vector3(value, value, value);
        }
    }
	// Use this for initialization
	void Awake () {
        
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

    void Reorient()
    {
        cap1.LookAt(cap2, Vector3.up);
        cap2.LookAt(cap1, Vector3.down);
        GetComponentInChildren<SkinnedMeshRenderer>().localBounds = new Bounds(Vector3.zero, cap1.localPosition - cap2.localPosition);
    }

}
