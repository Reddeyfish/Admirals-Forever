using UnityEngine;
using System.Collections;

public class Bullet : AbstractBullet {
    [SerializeField]
    protected float speed;

    [SerializeField]
    protected float spread;

    ParticlesObeyRotation particles;

    protected override void Awake()
    {
        base.Awake();
        particles = GetComponent<ParticlesObeyRotation>();
    }

    protected override void SetMotion(Ship target)
    {
        Vector2 targetVector = target.transform.position - this.transform.position;
        transform.rotation = targetVector.ToRotation() * Quaternion.AngleAxis(Random.Range(-spread, spread), Vector3.forward);
        particles.DoUpdate();
        rigid.velocity = speed * transform.right;
        Debug.Log(rigid.velocity);
    }
    protected override void SetTTL()
    {
        Callback.FireAndForget(() => SimplePool.Despawn(this.gameObject), range / speed, this);
    }

    protected override void OnHit(Collider other)
    {
        Section hitSection = other.GetComponent<Section>();
        if(hitSection && hitSection.Side != this.Side)
            SimplePool.Despawn(this.gameObject);
    }
}
