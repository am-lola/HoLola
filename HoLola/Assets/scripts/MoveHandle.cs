using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Persistence;

public class MoveHandle : Selectable {
    public Transform target;
    private Vector3 _basePos;

	public override void OnManipulateStart()
    {
        _basePos = target.position;

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
        target.position = _basePos + cumulativeDelta;
    }
}
