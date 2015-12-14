using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public abstract class AbstractBullet : AbstractShip {
    
    public float range {get; set;}

    public override float BaseHue
    {
        set 
        {
            base.baseHue = value;
        SetColor(HSVColor.HSVToRGB(baseHue, 1, 1));
        }
    }

    Vector2 target;
    public Vector2 Target
    {
        get { return target; }
        set 
        { 
            target = value;
            SetMotion(target);
            SetTTL();
        }
    }

    protected Rigidbody2D rigid;

    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    protected abstract void SetMotion(Vector2 target);
    protected abstract void SetTTL();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tags.ship))
            OnHit(other);
    }

    protected abstract void OnHit(Collider2D other);
    protected abstract void SetColor(Color color);
}
