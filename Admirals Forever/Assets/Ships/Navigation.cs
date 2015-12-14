using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(LineRenderer))]
public class Navigation : AbstractNavigation, IObserver<ShipDestroyedMessage>{

    [SerializeField]
    protected GameObject waypointUI;
    [SerializeField]
    protected GameObject facingUI;
    [SerializeField]
    protected GameObject attackUI;

    LineRenderer lineRenderer;
    const int numLineSegments = 100;

    public bool Selected
    {
        get { return lineRenderer.enabled; }
        set { lineRenderer.enabled = value; }
    }

    Queue<MovementOrderTuple> movementOrders = new Queue<MovementOrderTuple>();
    public void addMovementOrder(AbstractMovementOrder order) { if(!(order is AutomaticMovementOrder)) RemoveAutomaticOrders(); movementOrders.Enqueue(MovementOrderToTuple(order)); }
    public void clearMovementOrders() {
        foreach (MovementOrderTuple order in movementOrders)
            order.Despawn();
        movementOrders.Clear(); }

    List<AttackOrderTuple> attackOrders = new List<AttackOrderTuple>(); //use as a queue, but with random access to remove dead ships
    public void addAttackOrder(AttackOrder order) { if (!(order is AutomaticAttackOrder)) RemoveAutomaticOrders(); order.target.Subscribe<ShipDestroyedMessage>(this); attackOrders.Add(AttackOrderToTuple(order)); }
    public void clearAttackOrders()
    {
        foreach (AttackOrderTuple order in attackOrders)
            order.Despawn();
        attackOrders.Clear();
    }

    protected override void Awake()
    {
        base.Awake();
        lineRenderer = GetComponent<LineRenderer>();
        Bounds bounds = GetComponent<Renderer>().bounds;
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(rend.bounds);
        }
        float magnitude = bounds.extents.magnitude;
        lineRenderer.SetVertexCount(numLineSegments + 1);
        for (int i = 0; i <= numLineSegments; i++)
        {
            float angle = (2 * Mathf.PI * i) / numLineSegments;
            lineRenderer.SetPosition(i, magnitude * new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)));
        }
    }

    protected override void DoMovement()
    {
        if (movementOrders.Count != 0)
        {
            Vector2 movementDirection = movementOrders.Peek().order.Position - (Vector2)(transform.position);
            Assert.IsTrue(movementOrders.Peek().order.maxSpeed <= maxSpeed);
            Assert.IsTrue(movementOrders.Peek().order.accel <= accel);
            MoveTowards(movementDirection, movementOrders.Peek().order.maxSpeed, movementOrders.Peek().order.accel);
            if (rigid.velocity.magnitude * Time.fixedDeltaTime > movementDirection.magnitude) //we'll overshoot the next FixedUpdate
                movementOrders.Dequeue().Despawn();
        }
        else
        {
            MoveTowards(Vector2.zero);
        }
    }

    protected override void DoRotation()
    {
        if (movementOrders.Count != 0 && movementOrders.Peek().order is IFaceOrder)
        {
            RotateTowards((movementOrders.Peek().order as IFaceOrder).FacingDirection);
        }
        else if (attackOrders.Count != 0)
        {
            RotateTowards(attackOrders[0].order.target.transform.position - transform.position);
        }
        else if (movementOrders.Count != 0)
        {
            RotateTowards(movementOrders.Peek().order.Position - (Vector2)(transform.position));
        }
        else
        {
            Ship target = bestTarget();
            if(target != null)
            {
                addAttackOrder(new AutomaticAttackOrder(target));
                addMovementOrder(new AutomaticMovementOrder(target, preferredRange, this, MaxSpeed, Accel));
            }
        }
    }

    void Update()
    {
        if (movementOrders.Count != 0)
        {
            movementOrders.Peek().ui.Start = this.transform.position;
        }

        foreach (AttackOrderTuple order in attackOrders)
        {
            order.ui.Redraw();
        }
    }

    MovementOrderTuple MovementOrderToTuple(AbstractMovementOrder order)
    {
        WaypointUI ui = SimplePool.Spawn(waypointUI).GetComponent<WaypointUI>();
        MovementOrderTuple result;
        
        if (order is MoveFaceOrder)
        {
            FacingUI facing = SimplePool.Spawn(facingUI).GetComponent<FacingUI>();
            result = new MoveFaceOrderTuple(order, ui, facing);
            facing.Position = order.Position;
            facing.Direction = (order as MoveFaceOrder).FacingDirection;
        }
        else
        {
            result = new MovementOrderTuple(order, ui);
        }

        ui.Start = movementOrders.Count != 0 ? ui.Start = movementOrders.Last<MovementOrderTuple>().order.Position : (Vector2)(this.transform.position);
        ui.End = order.Position;
        return result;
    }

    AttackOrderTuple AttackOrderToTuple(AttackOrder order)
    {
        AttackUI ui = SimplePool.Spawn(attackUI).GetComponent<AttackUI>();
        ui.Source = this.transform;
        ui.Target = order.target.transform;
        return new AttackOrderTuple(order, ui);
    }

    void RemoveAutomaticOrders()
    {
        if (attackOrders.Count != 0 && attackOrders[0].order is AutomaticAttackOrder)
        {
            attackOrders[0].Despawn();
            attackOrders.RemoveAt(0);
        }

        if (movementOrders.Count != 0 && movementOrders.Peek().order is AutomaticMovementOrder)
        {
            movementOrders.Peek().Despawn();
            movementOrders.Dequeue();
        }
    }

    public override List<Ship> FilterTargets(HashSet<Ship> targets)
    {
        List<Ship> result = new List<Ship>();
        foreach (AttackOrderTuple order in attackOrders)
        {
            if (targets.Contains(order.order.target))
            {
                result.Add(order.order.target);
            }
        }
        return result;
    }

    public void Notify(ShipDestroyedMessage message)
    {
        for(int i = 0; i < attackOrders.Count; i++)
        {
            if (attackOrders[i].order.target == message.destroyedShip)
            {
                attackOrders[i].Despawn();
                attackOrders.RemoveAt(i);
            }
        }

        if (movementOrders.Count != 0 && movementOrders.Peek().order is AutomaticMovementOrder && (movementOrders.Peek().order as AutomaticMovementOrder).target == message.destroyedShip)
        {
            movementOrders.Peek().Despawn();
            movementOrders.Dequeue();
        }
    }

    void OnDestroy()
    {
        clearMovementOrders();
        clearAttackOrders();
    }
}

public class MovementOrderTuple
{
    public readonly AbstractMovementOrder order;
    public readonly WaypointUI ui;

    public MovementOrderTuple(AbstractMovementOrder order, WaypointUI ui)
    {
        this.order = order;
        this.ui = ui;
        if (order is AutomaticMovementOrder)
            (order as AutomaticMovementOrder).UI = ui;
    }

    public virtual void Despawn()
    {
        SimplePool.Despawn(ui.gameObject);
    }
}

public class MoveFaceOrderTuple : MovementOrderTuple
{
    public readonly FacingUI facingUI;

    public MoveFaceOrderTuple(AbstractMovementOrder order, WaypointUI ui, FacingUI facingUI)
        : base(order, ui)
    {
        this.facingUI = facingUI;
    }

    public override void Despawn()
    {
        base.Despawn();
        SimplePool.Despawn(facingUI.gameObject);
    }
}

public abstract class AbstractMovementOrder
{
    public abstract Vector2 Position {get;}
    public readonly float maxSpeed;
    public readonly float accel;
    public AbstractMovementOrder(float maxSpeed, float accel)
    {
        this.maxSpeed = maxSpeed;
        this.accel = accel;
    }
}

public class MovementOrder : AbstractMovementOrder
{
    readonly Vector2 position;
    public override Vector2 Position { get { return position; } }
    public MovementOrder(Vector2 position, float maxSpeed, float accel)
        :base(maxSpeed, accel)
    {
        this.position = position;
    }
}

public interface IFaceOrder
{
    Vector2 FacingDirection { get; }
}

public class MoveFaceOrder : MovementOrder, IFaceOrder
{
    readonly Vector2 facingDirection;
    public Vector2 FacingDirection { get { return facingDirection; } }

    public MoveFaceOrder(Vector2 position, float maxSpeed, float accel, Vector2 direction)
        : base(position, maxSpeed, accel)
    {
        this.facingDirection = direction;
    }
}

public class AutomaticMovementOrder : AbstractMovementOrder, IFaceOrder
{
    public readonly Ship target;
    public readonly float preferredRange;
    public readonly Navigation self;
    WaypointUI ui;
    public WaypointUI UI { set { ui = value; } }
    public override Vector2 Position { 
        get
        { 
            Vector2 displacement = self.transform.position - target.transform.position;
            Vector2 result = preferredRange * displacement.normalized + ((Vector2)(target.transform.position));
            ui.End = result;
            return result;
        }
    }
    public Vector2 FacingDirection { get { return target.transform.position - self.transform.position; } }
    public AutomaticMovementOrder(Ship target, float preferredRange, Navigation self, float maxSpeed, float accel)
        :base(maxSpeed, accel)
    {
        this.target = target;
        this.preferredRange = preferredRange;
        this.self = self;
    }
}

public class AttackOrderTuple
{
    public readonly AttackOrder order;
    public readonly AttackUI ui;

    public AttackOrderTuple(AttackOrder order, AttackUI ui)
    {
        this.order = order;
        this.ui = ui;
    }

    public virtual void Despawn()
    {
        SimplePool.Despawn(ui.gameObject);
    }
}

public class AttackOrder
{
    public readonly Ship target;

    public AttackOrder(Ship target)
    {
        this.target = target;
    }
}

public class AutomaticAttackOrder : AttackOrder
{
    public AutomaticAttackOrder(Ship target) : base(target) { }
}