using UnityEngine;
using System.Collections;

public class SingleBulletWeapon : ArcedWeapon {

    [SerializeField]
    protected GameObject bullet;

    [SerializeField]
    [AutoLink(childPath = "Visuals")]
    protected Transform visuals;

    protected override void Fire(Ship target)
    {
        FireBullet(target.transform.position);
        Active = false;
    }

    protected virtual void FireBullet(Vector2 target)
    {
        AbstractBullet spawnedBullet = SimplePool.Spawn(bullet, this.transform.position).GetComponent<AbstractBullet>();
        spawnedBullet.range = range;
        spawnedBullet.Target = target;
        spawnedBullet.Side = myShip.Side;
        spawnedBullet.BaseHue = myShip.BaseHue;

        visuals.rotation = (target - (Vector2)(this.transform.position)).ToRotation();
    }
}
