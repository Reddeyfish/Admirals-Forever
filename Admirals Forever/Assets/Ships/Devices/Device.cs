using UnityEngine;
using System.Collections;

public abstract class Device : MonoBehaviour {

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
    protected virtual void Start() { OnReady(); }
    protected virtual void OnActivate() { }
    protected virtual void OnDeactivate() { Callback.FireAndForget(() => Ready = true, cooldownTime, this); Debug.Log("Cooldown"); }
    protected virtual void OnReady() { }
    protected void Fire()
    {
        Active = true;
    }
}
