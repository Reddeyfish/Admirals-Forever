using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class Navigation : MonoBehaviour {

    [SerializeField]
    protected float maxSpeed;
    public float MaxSpeed { get { return maxSpeed; } }

    [SerializeField]
    protected float accel;
    public float Accel { get { return accel; } }

    [SerializeField]
    protected float rotationSpeed;
    public float RotationSpeed { get { return rotationSpeed; } }

    [SerializeField]
    protected GameObject waypointUI;
    [SerializeField]
    protected GameObject facingUI;
    [SerializeField]
    protected GameObject attackUI;

    Queue<MovementOrderTuple> movementOrders = new Queue<MovementOrderTuple>();
    public void addMovementOrder(MovementOrder order) { movementOrders.Enqueue(MovementOrderToTuple(order)); }
    public void clearMovementOrders() {
        foreach (MovementOrderTuple order in movementOrders)
            order.Despawn();
        movementOrders.Clear(); }

    Queue<AttackOrderTuple> attackOrders = new Queue<AttackOrderTuple>();
    public void addAttackOrder(AttackOrder order) { attackOrders.Enqueue(AttackOrderToTuple(order)); }
    public void clearAttackOrders()
    {
        foreach (AttackOrderTuple order in attackOrders)
            order.Despawn();
        attackOrders.Clear();
    }

    Rigidbody rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }
	// Use this for initialization

    void FixedUpdate()
    {
        DoMovement();
        //rotation
        DoRotation();
    }

    void DoMovement()
    {
        if (movementOrders.Count != 0)
        {
            Vector2 movementDirection = movementOrders.Peek().order.position - (Vector2)(transform.position);
            Assert.IsTrue(movementOrders.Peek().order.maxSpeed <= maxSpeed);
            Assert.IsTrue(movementOrders.Peek().order.accel <= accel);
            rigid.velocity = Vector2.MoveTowards(rigid.velocity, movementOrders.Peek().order.maxSpeed * movementDirection.normalized, movementOrders.Peek().order.maxSpeed * movementOrders.Peek().order.accel * Time.fixedDeltaTime);
            if (rigid.velocity.magnitude * Time.fixedDeltaTime > movementDirection.magnitude) //we'll overshoot the next FixedUpdate
                movementOrders.Dequeue().Despawn();
        }
        else
        {
            rigid.velocity = Vector2.MoveTowards(rigid.velocity, Vector2.zero, accel * Time.fixedDeltaTime);
        }
    }

    void DoRotation()
    {
        if (movementOrders.Count != 0 && movementOrders.Peek().order is MoveFaceOrder)
        {
            RotateTowards((movementOrders.Peek().order as MoveFaceOrder).facingDirection);
        }
        else if (attackOrders.Count != 0)
        {
            RotateTowards(attackOrders.Peek().order.target.transform.position - transform.position);
        }
        else if (movementOrders.Count != 0)
        {
            RotateTowards(movementOrders.Peek().order.position - (Vector2)(transform.position));
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

    void RotateTowards(Vector2 direction)
    {
        rigid.MoveRotation(Quaternion.RotateTowards(rigid.rotation, direction.ToRotation(), rotationSpeed * Time.fixedDeltaTime));
    }

    MovementOrderTuple MovementOrderToTuple(MovementOrder order)
    {
        WaypointUI ui = SimplePool.Spawn(waypointUI).GetComponent<WaypointUI>();
        ui.Start = movementOrders.Count != 0 ? ui.Start = movementOrders.Last<MovementOrderTuple>().order.position : (Vector2)(this.transform.position);
        ui.End = order.position;
        if (order is MoveFaceOrder)
        {
            FacingUI facing = SimplePool.Spawn(facingUI).GetComponent<FacingUI>();
            facing.Position = order.position;
            facing.Direction = (order as MoveFaceOrder).facingDirection;
            return new MoveFaceOrderTuple(order, ui, facing);
        }
        else
        {
            return new MovementOrderTuple(order, ui);
        }
    }

    AttackOrderTuple AttackOrderToTuple(AttackOrder order)
    {
        AttackUI ui = SimplePool.Spawn(attackUI).GetComponent<AttackUI>();
        ui.Source = this.transform;
        ui.Target = order.target.transform;
        return new AttackOrderTuple(order, ui);
    }

    public List<Ship> FilterTargets(HashSet<Ship> targets)
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
}

public class MovementOrderTuple
{
    public readonly MovementOrder order;
    public readonly WaypointUI ui;

    public MovementOrderTuple(MovementOrder order, WaypointUI ui)
    {
        this.order = order;
        this.ui = ui;
    }

    public virtual void Despawn()
    {
        SimplePool.Despawn(ui.gameObject);
    }
}

public class MoveFaceOrderTuple : MovementOrderTuple
{
    public readonly FacingUI facingUI;

    public MoveFaceOrderTuple(MovementOrder order, WaypointUI ui, FacingUI facingUI) : base(order, ui)
    {
        this.facingUI = facingUI;
    }

    public override void Despawn()
    {
        base.Despawn();
        SimplePool.Despawn(facingUI.gameObject);
    }
}


public class MovementOrder
{
    public readonly Vector2 position;
    public readonly float maxSpeed;
    public readonly float accel;
    public MovementOrder(Vector2 position, float maxSpeed, float accel)
    {
        this.position = position;
        this.maxSpeed = maxSpeed;
        this.accel = accel;
    }
}

public class MoveFaceOrder : MovementOrder
{
    public readonly Vector2 facingDirection;

    public MoveFaceOrder(Vector2 position, float maxSpeed, float accel, Vector2 direction)
        : base(position, maxSpeed, accel)
    {
        this.facingDirection = direction;
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