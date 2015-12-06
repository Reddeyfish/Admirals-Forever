using UnityEngine;
using System.Collections;

public abstract class ArcedWeapon : Weapon {

    [SerializeField]
    protected Vector2 direction;

    [SerializeField]
    protected float arc;

    protected override bool validTarget(Ship ship)
    {
        if (!base.validTarget(ship))
            return false;
        Vector2 targetDirection = ship.transform.position - this.transform.position;
        return Mathf.Abs(Vector3.Angle(transform.TransformVector(direction), targetDirection)) < arc;
    }
}
