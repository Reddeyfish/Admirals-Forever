using UnityEngine;
using System.Collections;

public abstract class Device : MonoBehaviour, IObserver<SectionDestroyedMessage> {

    [SerializeField]
    protected float cooldownTime;

    bool active = false;
    public bool Active
    {
        get
        {
            return active;
        }
        set
        {
            if (value)
            {
                if (!active)
                {
                    Ready = false;
                    OnActivate();
                }
            }
            else if (active)
            {
                OnDeactivate();
            }
            active = value;
        }
    }

    bool ready = true;
    public bool Ready
    {
        get
        {
            return ready;
        }
        set
        {
            if (!ready)
            {
                ready = value;
                if (ready)
                {
                    OnReady();
                }
            }
            else
            {
                ready = value;
            }
        }
    }
    protected virtual void Awake() { }
    protected virtual void Start() 
    {
        OnReady();
        foreach (IObservable<SectionDestroyedMessage> observable in GetComponentsInParent<IObservable<SectionDestroyedMessage>>())
        {
            observable.Subscribe(this);
        }
    }
    protected virtual void OnActivate() { }
    protected virtual void OnDeactivate() { Callback.FireAndForget(() => Ready = true, cooldownTime, this); }
    protected virtual void OnReady() { }
    protected void Fire()
    {
        Active = true;
    }

    public virtual void Notify(SectionDestroyedMessage message)
    {
        foreach (IObservable<SectionDestroyedMessage> observable in GetComponentsInParent<IObservable<SectionDestroyedMessage>>())
        {
            observable.Unsubscribe(this);
        }

        //there should't be any children, so it's ok to destroy it now
        DestroySelf();
    }

    protected virtual void DestroySelf()
    {
        Destroy(this.gameObject);
    }
}
