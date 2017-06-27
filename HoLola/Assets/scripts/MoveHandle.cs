using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Persistence;

public class MoveHandle : Selectable {
    public Transform target;
    private Vector3 _basePos;
    private WorldAnchorStore _anchorStore = null;

    private new void Start()
    {
        base.Start();
        WorldAnchorStore.GetAsync(OnWorldAnchorStoreReady);
    }

    private void OnWorldAnchorStoreReady(WorldAnchorStore store)
    {
        _anchorStore = store;
    }

    private void OnDestroy()
    {
        if (_anchorStore != null)
        {
            WorldAnchor anchor = target.GetComponent<WorldAnchor>();
            if (anchor != null)
            {
                _anchorStore.Save("lola_" + target.gameObject.name, anchor);
            }
        }
    }
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

        if (_anchorStore != null)
        {
            _anchorStore.Save("lola_" + target.gameObject.name, target.GetComponent<WorldAnchor>());
        }
    }

    public override void OnManipulate(Vector3 cumulativeDelta)
    {
        target.position = _basePos + cumulativeDelta;
    }
}
