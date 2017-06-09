using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA;

public class RotateHandle : Selectable
{
    public Transform target;
    public Vector3 axis;
    private Quaternion _baseRot;

    public override void OnManipulateStart()
    {
        _baseRot = target.rotation;

        var anchor = target.GetComponent<WorldAnchor>();
        if (anchor != null)
        {
            DestroyImmediate(anchor);
        }
    }

    public override void OnManipulateStop()
    {
        target.gameObject.AddComponent<WorldAnchor>();
    }

    public override void OnManipulate(Vector3 cumulativeDelta)
    {
        target.rotation = Quaternion.AngleAxis(cumulativeDelta.magnitude * 100.0f * Mathf.Sign(cumulativeDelta.y), target.rotation * axis) * _baseRot;
    }
}
