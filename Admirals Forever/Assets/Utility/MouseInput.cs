using UnityEngine;
using System.Collections;

public class MouseInput : MonoBehaviour, IObservable<DoubleLeftMouseClickMessage>, IObservable<LeftMouseDragStartMessage>, IObservable<LeftMouseDraggingMessage>, IObservable<LeftMouseDragEndMessage>,
    IObservable<RightMouseDragStartMessage>, IObservable<RightMouseDraggingMessage>, IObservable<RightMouseDragEndMessage>
{

    [SerializeField]
    protected float maxDoubleClickTime;

    Observable<DoubleLeftMouseClickMessage> doubleClickObservable = new Observable<DoubleLeftMouseClickMessage>();
    public Observable<DoubleLeftMouseClickMessage> Observable(IObservable<DoubleLeftMouseClickMessage> self) { return doubleClickObservable; }
    Observable<LeftMouseDragStartMessage> leftDragStartObservable = new Observable<LeftMouseDragStartMessage>();
    public Observable<LeftMouseDragStartMessage> Observable(IObservable<LeftMouseDragStartMessage> self) { return leftDragStartObservable; }
    Observable<LeftMouseDraggingMessage> leftDraggingObservable = new Observable<LeftMouseDraggingMessage>();
    public Observable<LeftMouseDraggingMessage> Observable(IObservable<LeftMouseDraggingMessage> self) { return leftDraggingObservable; }
    Observable<LeftMouseDragEndMessage> leftDragEndObservable = new Observable<LeftMouseDragEndMessage>();
    public Observable<LeftMouseDragEndMessage> Observable(IObservable<LeftMouseDragEndMessage> self) { return leftDragEndObservable; }
    Observable<RightMouseDragStartMessage> rightDragStartObservable = new Observable<RightMouseDragStartMessage>();
    public Observable<RightMouseDragStartMessage> Observable(IObservable<RightMouseDragStartMessage> self) { return rightDragStartObservable; }
    Observable<RightMouseDraggingMessage> rightDraggingObservable = new Observable<RightMouseDraggingMessage>();
    public Observable<RightMouseDraggingMessage> Observable(IObservable<RightMouseDraggingMessage> self) { return rightDraggingObservable; }
    Observable<RightMouseDragEndMessage> rightDragEndObservable = new Observable<RightMouseDragEndMessage>();
    public Observable<RightMouseDragEndMessage> Observable(IObservable<RightMouseDragEndMessage> self) { return rightDragEndObservable; }

    float lastClickTime = float.MinValue;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(LeftClickRoutine());
        }

        if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(RightClickRoutine());
        }
	}

    IEnumerator LeftClickRoutine()
    {
        Vector2 start = Input.mousePosition;
        Vector2 current = start;
        leftDragStartObservable.Post(new LeftMouseDragStartMessage(start));
        
        yield return null;

        while (Input.GetMouseButton(0))
        {
            current = Input.mousePosition;
            leftDraggingObservable.Post(new LeftMouseDraggingMessage(start, current));
            yield return null;
        }

        leftDragEndObservable.Post(new LeftMouseDragEndMessage(start, current));
        if (Time.realtimeSinceStartup - lastClickTime < maxDoubleClickTime)
        {
            doubleClickObservable.Post(new DoubleLeftMouseClickMessage(Input.mousePosition));
            lastClickTime = float.MinValue;
        }
        else
        {
            lastClickTime = Time.realtimeSinceStartup;
        }
    }

    IEnumerator RightClickRoutine()
    {
        Vector2 start = Input.mousePosition;
        Vector2 current = start;
        rightDragStartObservable.Post(new RightMouseDragStartMessage(start));

        yield return null;

        while (Input.GetMouseButton(1))
        {
            current = Input.mousePosition;
            rightDraggingObservable.Post(new RightMouseDraggingMessage(start, current));
            yield return null;
        }

        rightDragEndObservable.Post(new RightMouseDragEndMessage(start, current));
        lastClickTime = Time.realtimeSinceStartup;
    }
}

public class DoubleLeftMouseClickMessage
{
    public readonly Vector2 screenPoint;
    public readonly Vector2 worldPoint;

    public DoubleLeftMouseClickMessage(Vector2 screenPoint)
    {
        this.screenPoint = screenPoint;
        this.worldPoint = screenPoint.toWorldPoint();
    }
}

public class MouseDragStartMessage
{
    public readonly Vector2 point;
    public readonly Vector2 worldPoint;

    public MouseDragStartMessage(Vector2 point)
    {
        this.point = point;
        this.worldPoint = point.toWorldPoint();
    }
}

public class LeftMouseDragStartMessage : MouseDragStartMessage
{
    public LeftMouseDragStartMessage(Vector2 point) : base(point) { }
}

public class RightMouseDragStartMessage : MouseDragStartMessage
{
    public RightMouseDragStartMessage(Vector2 point) : base(point) { }
}

public class MouseDraggingMessage
{
    public readonly Vector2 start;
    public readonly Vector2 startWorldPoint;
    public readonly Vector2 current;
    public readonly Vector2 currentWorldPoint;

    public MouseDraggingMessage(Vector2 start, Vector2 current)
    {
        this.start = start;
        this.startWorldPoint = start.toWorldPoint();
        this.current = current;
        this.currentWorldPoint = current.toWorldPoint();
    }
}

public class LeftMouseDraggingMessage : MouseDraggingMessage
{
    public LeftMouseDraggingMessage(Vector2 start, Vector2 current) : base(start, current) { }
}

public class RightMouseDraggingMessage : MouseDraggingMessage
{
    public RightMouseDraggingMessage(Vector2 start, Vector2 current) : base(start, current) { }
}

public class MouseDragEndMessage
{
    public readonly Vector2 start;
    public readonly Vector2 startWorldPoint;
    public readonly Vector2 end;
    public readonly Vector2 endWorldPoint;

    public MouseDragEndMessage(Vector2 start, Vector2 end)
    {
        this.start = start;
        this.startWorldPoint = start.toWorldPoint();
        this.end = end;
        this.endWorldPoint = end.toWorldPoint();
    }
}

public class LeftMouseDragEndMessage : MouseDragEndMessage
{
    public LeftMouseDragEndMessage(Vector2 start, Vector2 end) : base(start, end) { }
}

public class RightMouseDragEndMessage : MouseDragEndMessage
{
    public RightMouseDragEndMessage(Vector2 start, Vector2 end) : base(start, end) { }
}