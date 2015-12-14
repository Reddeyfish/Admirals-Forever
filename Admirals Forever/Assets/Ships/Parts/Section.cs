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

    [SerializeField]
    protected float hueChangeTime;

    [SerializeField]
    protected float hueChangeTimeVariance;

    public int Side { get { return myShip.Side; } }

    float health;
    public float Health { get { return health; } }
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
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(ModifyHue());
        foreach (IObservable<SectionDestroyedMessage> observable in GetComponentsInParent<IObservable<SectionDestroyedMessage>>())
        {
            observable.Subscribe(this);
        }
    }

    IEnumerator ModifyHue()
    {
        float oldHue = myShip.getHue();
        for (; ; )
        {
            float newHue = myShip.getHue();
            yield return StartCoroutine(Callback.Routines.DoLerpRoutine((float l) => spriteRenderer.color = getColor(Mathf.Lerp(oldHue, newHue, l)), hueChangeTime + Random.value * hueChangeTimeVariance, this));
            oldHue = newHue;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag(Tags.ship))
        {
            Section hitSection = other.collider.GetComponent<Section>();
            if (hitSection && hitSection.Side != this.Side)
            {
                float damage = Mathf.Min(hitSection.Health, this.Health);
                hitSection.takeHit(damage);
                this.takeHit(damage);
            }
        }
    }

    public void takeHit(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            sectionDestroyedObservable.Post(new SectionDestroyedMessage(this));
        }
    }

    Color getColor(float myHue)
    {
        float healthFraction = health / maxHealth;
        return HSVColor.HSVToRGB(myHue, healthFraction, 0.5f * healthFraction + 0.5f);
    }

    public void Notify(SectionDestroyedMessage message)
    {
        foreach (IObservable<SectionDestroyedMessage> observable in GetComponentsInParent<IObservable<SectionDestroyedMessage>>())
        {
            if((Section)observable != this)
                observable.Unsubscribe(this);
        }

        if (health > 0)
        {
            sectionDestroyedObservable.Clear();
            this.Subscribe<SectionDestroyedMessage>(this);
            Ship newShip = gameObject.AddComponent<Ship>();
            newShip.Copy(myShip);
            myShip = newShip;
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