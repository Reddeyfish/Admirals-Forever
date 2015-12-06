using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public abstract class AbstractBullet : Ship {
    
    public float range {get; set;}

    Ship target;
    public Ship Target { get { return target; }
        set 
        { 
            target = value;
            SetMotion(target);
            SetTTL();
        }
    }

    protected Rigidbody rigid;

    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    protected abstract void SetMotion(Ship target);
    protected abstract void SetTTL();

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.ship))
            OnHit(other);
    }

    protected abstract void OnHit(Collider other);
}
