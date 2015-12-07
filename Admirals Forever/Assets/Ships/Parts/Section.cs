using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Section : MonoBehaviour, IObservable<SectionDestroyedMessage>, IObserver<SectionDestroyedMessage> {
    [SerializeField]
    protected float maxHealth;

    [SerializeField]
    protected float adriftSpeed;

    [SerializeField]
    protected float adriftRotation;

    public int Side { get { return myShip.Side; } }

    float health;
    public float Health { get { return health; } }
    float myHue;
    Ship myShip;
    SpriteRenderer spriteRenderer;
    Observable<SectionDestroyedMessage> sectionDestroyedObservable = new Observable<SectionDestroyedMessage>();
    public Observable<SectionDestroyedMessage> Observable(IObservable<SectionDestroyedMessage> self)
    {
        return sectionDestroyedObservable;
    }

    
	// Use this for initialization
	void Awake () {
        health = maxHealth;
	}

    void Start()
    {
        myShip = GetComponentInParent<Ship>();
        myHue = myShip.getHue();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = HSVColor.HSVToRGB(myHue, 1, 1);
        foreach (IObservable<SectionDestroyedMessage> observable in GetComponentsInParent<IObservable<SectionDestroyedMessage>>())
        {
            observable.Subscribe(this);
        }
        Debug.Log(Side);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("Ding");
    }

    public void takeHit(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            sectionDestroyedObservable.Post(new SectionDestroyedMessage(this));
        }
        spriteRenderer.color = getColor();
    }

    Color getColor()
    {
        float healthFraction = health / maxHealth;
        return HSVColor.HSVToRGB(myHue, healthFraction, 0.9f * healthFraction + 0.1f);
    }

    public void Notify(SectionDestroyedMessage message)
    {
        foreach (IObservable<SectionDestroyedMessage> observable in GetComponentsInParent<IObservable<SectionDestroyedMessage>>())
        {
            if((Section)observable != this)
                observable.Unsubscribe(this);
        }
        Callback.FireForUpdate(() =>
            {
                this.transform.SetParent(null, true);
                if (health <= 0)
                {
                    Callback.FireForUpdate(() => Destroy(this.gameObject), this);
                }
                else
                {
                    
                    Rigidbody2D rigid = gameObject.AddComponent<Rigidbody2D>();
                    rigid.gravityScale = 0;
                    rigid.velocity = myShip.GetComponent<Rigidbody2D>().velocity + adriftSpeed * (Vector2)(this.transform.position - message.destroyedSection.transform.position).normalized;
                    rigid.angularVelocity = Random.Range(-adriftRotation, adriftRotation);

                    Ship newShip = gameObject.AddComponent<Ship>();
                    newShip.Copy(myShip);
                    myShip = newShip;
                }

            }, this, Callback.Mode.FIXEDUPDATE);
    }


}

public class SectionDestroyedMessage
{
    public readonly Section destroyedSection;

    public SectionDestroyedMessage(Section destroyedSection)
    {
        this.destroyedSection = destroyedSection;
    }
}