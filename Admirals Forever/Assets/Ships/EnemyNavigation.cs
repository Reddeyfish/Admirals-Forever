using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class EnemyNavigation : AbstractNavigation, IObserver<ShipDestroyedMessage> {

    [SerializeField]
    protected float searchInterval;

    Ship myTarget;
    Countdown targetSearchingCountdown;

    protected override void Awake()
    {
        base.Awake();
        targetSearchingCountdown = Countdown.TimedCountdown(FindNewTarget, searchInterval, this);
    }

    protected override void Start()
    {
        base.Start();
        FindNewTarget();
    }

    void FindNewTarget()
    {
        myTarget = bestTarget(myTarget);
        if (myTarget != null)
            myTarget.Subscribe<ShipDestroyedMessage>(this);
        else
        {
            targetSearchingCountdown.Restart();
        }
    }

    protected override void DoMovement()
    {
        Vector3 targetVelocity = Vector3.zero;
        if (myTarget != null)
        {
            Vector2 displacement = this.transform.position - myTarget.transform.position;
            targetVelocity = preferredRange * displacement.normalized + ((Vector2)(myTarget.transform.position)) - ((Vector2)(this.transform.position));
        }
        MoveTowards(targetVelocity);
    }

    protected override void DoRotation()
    {
        if (myTarget != null)
        {
            RotateTowards(myTarget.transform.position - this.transform.position);
        }
    }

    public void Notify(ShipDestroyedMessage message)
    {
        FindNewTarget();
    }

    void OnDestroy()
    {
        if (myTarget != null)
            myTarget.Unsubscribe<ShipDestroyedMessage>(this);
    }

    public override List<Ship> FilterTargets(HashSet<Ship> targets)
    {
        List<Ship> result = new List<Ship>();
        if (targets.Contains(myTarget))
            result.Add(myTarget);
        return result;
    }
}
