using UnityEngine;
using UnityEngine.VR.WSA.Input;

public class GazeGestureManager : MonoBehaviour
{
    public static GazeGestureManager Instance { get; private set; }

    // Represents the hologram that is currently focused.
    public GameObject FocusedObject { get; private set; }
    public bool HasSelection { get; private set; }

    GestureRecognizer recognizer;

    // Use this for initialization
    void Awake()
    {
        Instance = this;

        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.SetRecognizableGestures(GestureSettings.Tap | GestureSettings.ManipulationTranslate);
        recognizer.ManipulationStartedEvent += Recognizer_ManipulationStartedEvent;
        recognizer.ManipulationCompletedEvent += Recognizer_ManipulationCompletedEvent;
        recognizer.ManipulationCanceledEvent += Recognizer_ManipulationCompletedEvent;
        recognizer.ManipulationUpdatedEvent += Recognizer_ManipulationUpdatedEvent;
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            if (FocusedObject != null)  // de-select previously selected object
            {
                var selectable = FocusedObject.GetComponent<Selectable>();
                if (selectable != null)
                {
                    if (HasSelection)
                    {
                        selectable.Deselect(); // notify object of deselection
                        FocusedObject = null;
                        HasSelection = false;
                    }
                    else // select new object
                    {
                        selectable.Select(); // notify object of selection
                        HasSelection = true;
                    }
                }
            }
        };

        recognizer.StartCapturingGestures();
    }

    private void Recognizer_ManipulationCompletedEvent(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        if (FocusedObject != null && HasSelection)
        {
            var selectable = FocusedObject.GetComponent<Selectable>();
            if (selectable != null)
                selectable.OnManipulateStop();
        }
    }

    private void Recognizer_ManipulationStartedEvent(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        if (FocusedObject != null && HasSelection)
        {
            var selectable = FocusedObject.GetComponent<Selectable>();
            if (selectable != null)
                selectable.OnManipulateStart();
        }
    }

    private void Recognizer_ManipulationUpdatedEvent(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        if (FocusedObject != null && HasSelection)
        {
            var selectable = FocusedObject.GetComponent<Selectable>();
            if (selectable != null)
                selectable.OnManipulate(cumulativeDelta);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (HasSelection) // if we've selected an object, leave it as the focused object
        {
            return;
        }

        // Figure out which hologram is focused this frame.
        GameObject oldFocusObject = FocusedObject;

        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;
        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
        {
            // If the raycast hit a hologram, use that as the focused object.
            FocusedObject = hitInfo.collider.gameObject;
        }
        else
        {
            // If the raycast did not hit a hologram, clear the focused object.
            FocusedObject = null;
        }

        // If the focused object changed this frame,
        // start detecting fresh gestures again.
        if (FocusedObject != oldFocusObject)
        {
            recognizer.CancelGestures();
            recognizer.StartCapturingGestures();
        }
    }
}