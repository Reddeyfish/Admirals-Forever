using UnityEngine;
using System.Collections;
[RequireComponent(typeof(ParticlesObeyRotation))]
[RequireComponent(typeof(SpriteRenderer))]
public class Bullet : AbstractBullet {
    [SerializeField]
    protected float speed;

    [SerializeField]
    protected float spread;

    [SerializeField]
    protected float damage;

    ParticlesObeyRotation particles;
    SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        particles = GetComponent<ParticlesObeyRotation>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void SetMotion(Ship target)
    {
        Vector2 targetVector = target.transform.position - this.transform.position;
        transform.rotation = targetVector.ToRotation() * Quaternion.AngleAxis(Random.Range(-spread, spread), Vector3.forward);
        particles.DoUpdate();
        rigid.velocity = speed * transform.right;
    }
    protected override void SetTTL()
    {
        Callback.FireAndForget(() => SimplePool.Despawn(this.gameObject), range / speed, this);
    }

    protected override void OnHit(Collider2D other)
    {
        Section hitSection = other.GetComponent<Section>();
        if (hitSection && hitSection.Side != this.Side)
        {
            hitSection.takeHit(damage);
            SimplePool.Despawn(this.gameObject);
        }
    }

    protected override void SetColor(Color color)
    {
        spriteRenderer.color = color;
        particles.ParticleSystem.startColor = color;
    }
}
