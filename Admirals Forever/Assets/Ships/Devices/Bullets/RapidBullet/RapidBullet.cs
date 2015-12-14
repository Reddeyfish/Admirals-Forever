using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class RapidBullet : StandardBullet
{
    [SerializeField]
    protected float speed;

    [SerializeField]
    protected float spread;

    SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void SetMotion(Vector2 target)
    {
        Vector2 targetVector = target - (Vector2)(this.transform.position);
        transform.rotation = targetVector.ToRotation();
        rigid.velocity = speed * transform.right;
        transform.position += (Vector3)(spread * Random.insideUnitCircle);
    }
    protected override void SetTTL()
    {
        Callback.FireAndForget(() => SimplePool.Despawn(this.gameObject), range / speed, this);
    }

    protected override void SetColor(Color color)
    {
    }
}
