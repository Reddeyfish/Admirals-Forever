using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class AbstractNavigation : MonoBehaviour, IObserver<SectionDestroyedMessage>, IObserver<SelfShipDestroyedMessage>
{
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
    protected float maxRange;

    protected Rigidbody2D rigid;

    protected Quaternion rotation;

    protected AbstractShip myShip;

    protected float preferredRange;

    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        myShip = GetComponent<AbstractShip>();
        myShip.Subscribe<SelfShipDestroyedMessage>(this);
    }

    protected virtual void Start()
    {
        rotation = Quaternion.Euler(0, 0, rigid.rotation);
        Section mySection = GetComponent<Section>();
        foreach (Section observable in GetComponentsInChildren<Section>())
        {
            if(observable != mySection)
                observable.Subscribe<SectionDestroyedMessage>(this);
        }
        CalculatePreferredRange();
    }

    protected void CalculatePreferredRange()
    {
        Weapon[] weapons = GetComponentsInChildren<Weapon>();
        if (weapons.Length == 0)
            preferredRange = 0; //ramming speed!
        else
        {
            float minRange = weapons[0].Range;
            float sumRange = minRange;
            for (int i = 1; i < weapons.Length; i++)
            {
                sumRange += weapons[i].Range;
                if (weapons[i].Range < minRange)
                    minRange = weapons[i].Range;
            }
            preferredRange = Mathf.Min(minRange, sumRange / (3 * weapons.Length));
        }
    }

    void FixedUpdate()
    {
        DoMovement();
        //rotation
        DoRotation();
    }

    protected abstract void DoMovement();

    protected abstract void DoRotation();

    protected void RotateTowards(Vector2 direction)
    {
        rotation = Quaternion.RotateTowards(rotation, direction.ToRotation(), rotationSpeed * Time.fixedDeltaTime);
        transform.rotation = rotation;
        //rigid.MoveRotation(rotation.ToZRotation());
    }

    protected void MoveTowards(Vector2 direction)
    {
        MoveTowards(direction, MaxSpeed, Accel);
    }

    protected void MoveTowards(Vector2 direction, float maxSpeed, float accel)
    {
        rigid.velocity = Vector2.MoveTowards(rigid.velocity, maxSpeed * direction.normalized, maxSpeed * accel * Time.fixedDeltaTime);
    }

    public abstract List<Ship> FilterTargets(HashSet<Ship> targets);

    protected Ship bestTarget(Ship avoid = null)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(this.transform.position, maxRange);
        List<Ship> targets = new List<Ship>();
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].CompareTag(Tags.ship))
            {
                Ship ship = hits[i].GetComponentInParent<Ship>();
                if (ship != null && ship.Side != myShip.Side)
                    targets.Add(ship);
            }
        }
        if (targets.Count != 0)
        {
            Ship bestTarget = targets[0];
            float bestAngle = Vector2.Angle(this.transform.right, bestTarget.transform.position - this.transform.position);
            for (int i = 1; i < targets.Count; i++)
            {
                if (targets[i] == avoid)
                    continue;
                float newAngle = Vector2.Angle(this.transform.right, targets[i].transform.position - this.transform.position);
                Assert.IsTrue(newAngle >= 0);
                if (newAngle < bestAngle)
                {
                    newAngle = bestAngle;
                    bestTarget = targets[i];
                }
            }
            if(bestTarget != avoid)
                return bestTarget;
        }
        return null;
    }

    public void Notify(SectionDestroyedMessage message)
    {
        Vector2 newVelocity = MaxSpeed * 3 * (this.transform.position - message.destroyedSection.transform.position).normalized;
        Callback.FireForUpdate(
            () => { CalculatePreferredRange(); Callback.FireForUpdate(() => rigid.velocity = newVelocity, this, Callback.Mode.FIXEDUPDATE); }
            , this, Callback.Mode.FIXEDUPDATE);
    }

    public void Notify(SelfShipDestroyedMessage message) { Destroy(this); }
}