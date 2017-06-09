using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Selectable : MonoBehaviour {
    private MeshRenderer _renderer;
    private bool _selected = false;

	void Start () {
        _renderer = GetComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator SelectionPulse()
    {
        _selected = true;
        var baseColor = _renderer.material.color;
        var baseEmission = _renderer.material.GetColor("_EmissionColor");
        var hiColor = new Color(Mathf.Min(1.0f, baseColor.r + 0.25f),
                                Mathf.Min(1.0f, baseColor.g + 0.20f),
                                Mathf.Min(1.0f, baseColor.b + 0.05f));
        float t = 0.0f;
        float half_delta = 0.3f;
        float dir = 1.0f;
        while (_selected)
        {
            if (t < 0 || t > half_delta)
            {
                dir *= -1;
            }

            t += Time.deltaTime * dir;
            var newColor =
            _renderer.material.color = Color.Lerp(baseColor, hiColor, t);
            _renderer.material.SetColor("_EmissionColor", Color.Lerp(baseEmission, hiColor, t));
            yield return new WaitForSeconds(0.01f);
        }

        _renderer.material.color = baseColor; // reset color
        _renderer.material.SetColor("_EmissionColor", baseEmission);

    }
    public virtual void Select()
    {
        Debug.Log(gameObject.name + " Selected");
        if (!_selected)
        {
            StartCoroutine(SelectionPulse());
        }
    }

    public virtual void Deselect()
    {
        _selected = false;
        Debug.Log(gameObject.name + " DeSelected");
    }

    public virtual void OnManipulateStart()
    {

    }
    public virtual void OnManipulate(Vector3 cumulativeDelta)
    {
        Debug.Log(gameObject.name + " Manipulation delta: " + cumulativeDelta);
    }
}
