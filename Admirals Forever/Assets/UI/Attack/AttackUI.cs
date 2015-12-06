using UnityEngine;
using System.Collections;

public class AttackUI : MonoBehaviour {

    Transform target;
    public Transform Target
    {
        get { return target; }
        set
        {
            target = value;
            lineRenderer.SetPosition(1, target.position);
        }
    }

    Transform source;
    public Transform Source
    {
        get { return source; }
        set
        {
            source = value;
            lineRenderer.SetPosition(0, source.position);
        }
    }

    LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Redraw() //call every update
    {
        lineRenderer.SetPosition(0, source.position);
        lineRenderer.SetPosition(1, target.position);
    }
}
