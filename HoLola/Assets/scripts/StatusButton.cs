using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GUI Control
/// Handles display of simple status information for networked components
/// </summary>
public class StatusButton : MonoBehaviour {

    public Sprite good;
    public Sprite bad;
    public Sprite neutral;

    public Image UIImage;

    public enum Status
    {
        Neutral,
        Good,
        Bad,
        Error,
        Unknown
    }

    private Status _status;
    public Status status
    {
        set
        {
            _status = value;
            switch (_status)
            {
                case Status.Neutral:
                    UIImage.sprite = neutral;
                    break;
                case Status.Good:
                    UIImage.sprite = good;
                    break;
                case Status.Bad:
                    UIImage.sprite = bad;
                    break;
                case Status.Error:
                    UIImage.sprite = bad;
                    break;
                case Status.Unknown:
                    UIImage.sprite = neutral;
                    break;
            }
        }

        get { return _status; }
    }
}
