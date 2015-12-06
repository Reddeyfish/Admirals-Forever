using UnityEngine;
using System.Collections;

public class WaypointUI : MonoBehaviour {

    Vector2 end;
    public Vector2 End { get { return end; }
        set
        {
            end = value;
            this.transform.position = end;
            lineRenderer.SetPosition(1, end);
        }
    }

    Vector2 start;
    public Vector2 Start
    {
        get { return start; }
        set
        {
            start = value;
            lineRenderer.SetPosition(0, value);
        }
    }

    LineRenderer lineRenderer;

	// Use this for initialization
	void Awake () {
        lineRenderer = GetComponent<LineRenderer>();
	}
}