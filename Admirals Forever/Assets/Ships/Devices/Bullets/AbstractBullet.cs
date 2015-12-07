using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public abstract class AbstractBullet : Ship {
    
    public float range {get; set;}

    public override float BaseHue
    {
        set 
        {
            base.baseHue = value;
        SetColor(HSVColor.HSVToRGB(baseHue, 1, 1));
        }
    }

    Ship target;
    public Ship Target { get { return target; }
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

    protected abstract void SetMotion(Ship target);
    protected abstract void SetTTL();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tags.ship))
            OnHit(other);
    }

    protected abstract void OnHit(Collider2D other);
    protected abstract void SetColor(Color color);
}
