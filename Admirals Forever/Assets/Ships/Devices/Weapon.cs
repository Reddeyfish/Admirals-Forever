using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public abstract class Weapon : Device {
    [SerializeField]
    protected float range;

    protected HashSet<Ship> targets = new HashSet<Ship>();

    Navigation navigation;
    protected Ship myShip;

    protected override void Awake()
    {
        base.Awake();
        GetComponent<SphereCollider>().radius = range;
    }

    protected override void Start()
    {
        navigation = GetComponentInParent<Navigation>();
        myShip = GetComponentInParent<Ship>();
        foreach (Collider coll in Physics.OverlapSphere(this.transform.position, range))
        {
            OnTriggerEnter(coll);
        }
        base.Start();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(Tags.ship))
            return;
        Ship otherShip = other.GetComponentInParent<Ship>();
        if (otherShip != null)
            targets.Add(otherShip);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(Tags.ship))
            return;
        Ship otherShip = other.GetComponentInParent<Ship>();
        if (otherShip != null)
            targets.Remove(otherShip);
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
