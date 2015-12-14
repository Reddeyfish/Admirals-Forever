using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class AbstractShip : MonoBehaviour, IObservable<ShipDestroyedMessage> {

    [CanBeDefaultOrNull]
    [SerializeField]
    protected int side; //0 is player
    public int Side { 
        get { return side; }
        set { side = value; }
    }

    [CanBeDefaultOrNull]
    [SerializeField]
    protected float baseHue;
    public virtual float BaseHue
    {
        get { return baseHue; }
        set { baseHue = value; }
    }

    [CanBeDefaultOrNull]
    [SerializeField]
    protected float hueVariance;
    public float HueVariance
    {
        get { return hueVariance; }
        set { hueVariance = value; }
    }

    protected Observable<ShipDestroyedMessage> shipDestroyedObservable = new Observable<ShipDestroyedMessage>();
    public Observable<ShipDestroyedMessage> Observable(IObservable<ShipDestroyedMessage> self) { return shipDestroyedObservable; }

    public float getHue()
    {
        return (baseHue + Random.Range(-hueVariance, hueVariance)) % 1;
    }

    public void Copy(Ship ship)
    {
        this.Side = ship.Side;
        this.BaseHue = ship.BaseHue;
        this.HueVariance = ship.HueVariance;
    }
}

[RequireComponent(typeof(Section))]
public class Ship : AbstractShip, IObserver<SectionDestroyedMessage>
{
    void Awake()
    {
        GetComponent<Section>().Subscribe(this);
    }

    void Start()
    {
        //disable self-ship collisions
        Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();
        List<Collider2D> validColliders = new List<Collider2D>(childColliders.Where((Collider2D col) => !col.isTrigger));
        foreach (Collider2D coll1 in validColliders)
            foreach (Collider2D coll2 in validColliders)
                Physics2D.IgnoreCollision(coll1, coll2);
    }

    public void Notify(SectionDestroyedMessage message)
    {
        shipDestroyedObservable.Post(new ShipDestroyedMessage(this));
        Destroy(this);
    }
}

public class ShipDestroyedMessage
{
    public readonly AbstractShip destroyedShip;

    public ShipDestroyedMessage(AbstractShip destroyedShip)
    {
        this.destroyedShip = destroyedShip;
    }
}