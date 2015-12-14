using UnityEngine;
using System.Collections;

public abstract class StandardBullet : AbstractBullet
{
    [SerializeField]
    protected float damage;

    protected override void OnHit(Collider2D other)
    {
        Section hitSection = other.GetComponent<Section>();
        if (hitSection && hitSection.Side != this.Side)
        {
            hitSection.takeHit(damage);
            SimplePool.Despawn(this.gameObject);
        }
    }
}
