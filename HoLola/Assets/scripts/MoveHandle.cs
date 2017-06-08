using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveHandle : Selectable {
    public Transform target;
    private Vector3 _basePos;

	public override void OnManipulateStart()
    {
        _basePos = target.position;
    }

    public override void OnManipulate(Vector3 cumulativeDelta)
    {
        target.position = _basePos + cumulativeDelta;
    }
}
