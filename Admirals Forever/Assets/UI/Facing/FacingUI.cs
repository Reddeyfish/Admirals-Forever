using UnityEngine;
using System.Collections;

public class FacingUI : MonoBehaviour {

    Vector2 direction;
    public Vector2 Direction
    {
        get { return direction; }
        set
        {
            direction = value;
            lineRenderer.SetPosition(1, direction.normalized);
        }
    }

    Vector2 position;
    public Vector2 Position
    {
        get { return position; }
        set
        {
            position = value;
            this.transform.position = position;
        }
    }

    LineRenderer lineRenderer;

    // Use this for initialization
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
}
