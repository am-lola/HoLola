using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Persistence;
using HoloToolkit.Unity;

public class RotateHandle : Selectable
{
    public Transform target;
    public Vector3 axis;
    public float sensitivity = 200.0f;
    private Quaternion _baseRot;
    private WorldAnchorManager _anchorStore = null;
    private string _anchorName;

    private new void Start()
    {
        base.Start();
        _anchorStore = WorldAnchorManager.Instance;
        _anchorName = "lola_" + target.gameObject.name;
        if (_anchorStore == null)
        {
            Debug.LogError("This scripts expects a WorldAnchorManager component to exist in the scene");
        }
    }

    //private void OnDestroy()
    //{
    //    if (_anchorStore != null)
    //    {
    //        WorldAnchor anchor = target.GetComponent<WorldAnchor>();
    //        if (anchor != null)
    //        {
    //            _anchorStore.Save("lola_" + target.gameObject.name, anchor);
    //        }
    //    }
    //}

    public override void OnManipulateStart()
    {
        _baseRot = target.rotation;
        _anchorStore.RemoveAnchor(target.gameObject);

        //var anchor = target.GetComponent<WorldAnchor>();
        //if (anchor != null)
        //{
        //    DestroyImmediate(anchor);
        //}
    }

    public override void OnManipulateStop()
    {
        _anchorStore.AttachAnchor(target.gameObject, _anchorName);

        //target.gameObject.AddComponent<WorldAnchor>();

        //if (_anchorStore != null)
        //{
        //    _anchorStore.Save("lola_" + target.gameObject.name, target.GetComponent<WorldAnchor>());
        //}
    }

    public override void OnManipulate(Vector3 cumulativeDelta)
    {
        target.rotation = Quaternion.AngleAxis(cumulativeDelta.magnitude * sensitivity * Mathf.Sign(cumulativeDelta.y), target.rotation * axis) * _baseRot;
    }
}
