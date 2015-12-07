using UnityEngine;
using System.Collections;

public class SingleBulletWeapon : ArcedWeapon {

    [SerializeField]
    protected GameObject bullet;

    protected override void Fire(Ship target)
    {
        FireBullet(target);
        Active = false;
    }

    protected virtual void FireBullet(Ship target)
    {
        AbstractBullet spawnedBullet = SimplePool.Spawn(bullet, this.transform.position).GetComponent<Bullet>();
        spawnedBullet.range = range;
        spawnedBullet.Target = target;
        spawnedBullet.Side = myShip.Side;
        spawnedBullet.BaseHue = myShip.BaseHue;
    }
}
