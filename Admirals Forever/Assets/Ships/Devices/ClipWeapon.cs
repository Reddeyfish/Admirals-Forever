using UnityEngine;
using System.Collections;

public class ClipWeapon : SingleBulletWeapon {

    [SerializeField]
    protected int bulletsPerClip;

    [SerializeField]
    protected float bulletCycleTime;

    [SerializeField]
    protected float strafe;

    protected override void Fire(Ship target)
    {
        StartCoroutine(FireClip(target.transform.position));
    }

    IEnumerator FireClip(Vector2 target)
    {
        Vector2 displacement = target - (Vector2)(this.transform.position);
        displacement = strafe * new Vector2(displacement.y, -displacement.x).normalized;
        if (Random.value >= 0.5f)
            displacement = -displacement;
        FireBullet(target + displacement);
        for (int i = 1; i < bulletsPerClip; i++)
        {
            yield return new WaitForSeconds(bulletCycleTime);
            FireBullet(target + Vector2.Lerp(displacement, -displacement, (float)(i) / bulletsPerClip));
        }
        Active = false;
    }
}
