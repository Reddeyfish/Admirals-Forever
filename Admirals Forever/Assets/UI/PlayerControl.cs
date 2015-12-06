using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class PlayerControl : MonoBehaviour, IObserver<DoubleLeftMouseClickMessage>, IObserver<LeftMouseDragStartMessage>, IObserver<LeftMouseDraggingMessage>, IObserver<LeftMouseDragEndMessage>,
    IObserver<RightMouseDragStartMessage>, IObserver<RightMouseDraggingMessage>, IObserver<RightMouseDragEndMessage>
{
    [SerializeField]
    protected GameObject enemyShip;

    [SerializeField]
    protected float dragVsClickTolerance;

    [SerializeField]
    [AutoLink]
    protected MouseInput input;

    [SerializeField]
    [AutoLink(parentName=Tags.canvas, parentTag=Tags.canvas)]
    protected Transform canvas;

    public Navigation selectedShip;

    LineRenderer lineRenderer;
	// Use this for initialization
	void Awake () {
        input.Subscribe<DoubleLeftMouseClickMessage>(this);
        input.Subscribe<LeftMouseDragStartMessage>(this);
        input.Subscribe<LeftMouseDraggingMessage>(this);
        input.Subscribe<LeftMouseDragEndMessage>(this);
        input.Subscribe<RightMouseDragStartMessage>(this);
        input.Subscribe<RightMouseDraggingMessage>(this);
        input.Subscribe<RightMouseDragEndMessage>(this);
        lineRenderer = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Notify(DoubleLeftMouseClickMessage m)
    {
        Ship spawnedShip = SimplePool.Spawn(enemyShip, m.worldPoint).GetComponent<Ship>();
        spawnedShip.Side = 1;
        selectedShip.addAttackOrder(new AttackOrder(spawnedShip));
    }

    public void Notify(LeftMouseDragStartMessage m)
    {

    }

    public void Notify(LeftMouseDraggingMessage m)
    {

    }

    public void Notify(LeftMouseDragEndMessage m)
    {

    }

    public void Notify(RightMouseDragStartMessage m)
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, m.worldPoint);
        lineRenderer.SetPosition(1, m.worldPoint);
    }

    public void Notify(RightMouseDraggingMessage m)
    {
        if ((m.current - m.start).magnitude > dragVsClickTolerance)
        {
            //if it's large enough to count as a drag
            lineRenderer.SetPosition(1, m.currentWorldPoint);
        }
        else
        {
            lineRenderer.SetPosition(1, m.startWorldPoint);
        }
    }

    public void Notify(RightMouseDragEndMessage m)
    {
        lineRenderer.enabled = false;
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            selectedShip.clearMovementOrders();
        }
        if ((m.end - m.start).magnitude < dragVsClickTolerance)
        {
            //then treat it as a click
            selectedShip.addMovementOrder(new MovementOrder(m.end.toWorldPoint(), selectedShip.MaxSpeed, selectedShip.Accel));
        }
        else
        {
            //then treat it as a drag
            selectedShip.addMovementOrder(new MoveFaceOrder(m.start.toWorldPoint(), selectedShip.MaxSpeed, selectedShip.Accel, m.end.toWorldPoint() - m.start.toWorldPoint()));
        }
    }
}
