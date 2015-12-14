using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
public abstract class Weapon : Device, IObserver<ShipDestroyedMessage> {
    [SerializeField]
    protected float range;
    public float Range { get { return range; } }

    protected HashSet<Ship> targets = new HashSet<Ship>();

    AbstractNavigation navigation;
    protected Ship myShip;

    protected override void Awake()
    {
        base.Awake();
        GetComponent<CircleCollider2D>().radius = range;
    }

    protected override void Start()
    {
        navigation = GetComponentInParent<AbstractNavigation>();
        myShip = GetComponentInParent<Ship>();
        foreach (Collider2D coll in Physics2D.OverlapCircleAll(this.transform.position, range))
        {
            OnTriggerEnter2D(coll);
        }
        base.Start();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(Tags.ship))
            return;
        Ship otherShip = other.GetComponentInParent<Ship>();
        if (otherShip != null)
        {
            targets.Add(otherShip);
            otherShip.Subscribe<ShipDestroyedMessage>(this);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(Tags.ship))
            return;
        Ship otherShip = other.GetComponentInParent<Ship>();
        if (otherShip != null)
        {
            targets.Remove(otherShip);
            otherShip.Unsubscribe<ShipDestroyedMessage>(this);
        }
    }

    public virtual void Notify(ShipDestroyedMessage message)
    {
        targets.Remove(message.destroyedShip as Ship);
    }

    protected override void OnReady()
    {
        base.OnReady();
        StartCoroutine(SearchForTargets());
    }

    IEnumerator SearchForTargets()
    {
        for (; ; )
        {
            List<Ship> validTargets = navigation.FilterTargets(targets);
            if (validTargets.Count == 0)
            {
                //no navigation target matches
                foreach (Ship target in targets)
                    if (validTarget(target))
                        validTargets.Add(target);
            }
            else
            {
                //filter the navigation's targets
                for (int i = validTargets.Count - 1; i >= 0; i--)
                {
                    if (!validTarget(validTargets[i]))
                        validTargets.RemoveAt(i);
                }
            }

            if (validTargets.Count != 0)
            {
                Fire();
                Fire(findBestTarget(validTargets));
                yield break;
            }
            else
            {
                //nothing found, try again next frame
                yield return new WaitForFixedUpdate();
            }
        }
    }

    Ship findBestTarget(List<Ship> validTargets)
    {
        Ship bestTarget = validTargets[0];
        float bestTargetDistanceSqr = (bestTarget.transform.position - this.transform.position).sqrMagnitude;
        for (int i = 1; i < validTargets.Count; i++)
        {
            float newTargetDistanceSqr = (validTargets[i].transform.position - this.transform.position).sqrMagnitude;
            if (newTargetDistanceSqr < bestTargetDistanceSqr)
            {
                bestTarget = validTargets[i];
                bestTargetDistanceSqr = newTargetDistanceSqr;
            }
        }
        return bestTarget;
    }

    protected virtual bool validTarget(Ship ship)
    {
        return ship.Side != myShip.Side;
    }

    protected abstract void Fire(Ship target);
}
