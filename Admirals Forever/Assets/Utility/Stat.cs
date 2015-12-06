using UnityEngine;
using System.Collections.Generic;

public class FloatStatTracker
{
    public delegate void Recalculate();

    private bool dirty;
    private float _cachedValue;
    public float value 
    {
        get {
            if (dirty)
                recalculateCache();
            return _cachedValue; 
        } 
    }

    //private readonly Recalculate externalUpdate;
    private readonly float baseValue;
    private readonly Recalculate internalUpdate; //update internal values; function depends on if we can wait on dirty, or if we have to update externally
    private List<FloatStat> stats = new List<FloatStat>();

    private void recalculateCache()
    {
        _cachedValue = baseValue;
        foreach (float i in stats)
        {
            _cachedValue *= i;
        }
        dirty = false;
    }

    private void setDirty()
    {
        dirty = true;
    }

    public FloatStatTracker(float initValue)
    {
        baseValue = initValue;
        internalUpdate = setDirty;
        internalUpdate();
    }

    public FloatStatTracker(float initValue, Recalculate update)
    {
        baseValue = initValue;
        internalUpdate = recalculateCache + update;
        dirty = false;
        _cachedValue = initValue;
    }

    public FloatStat addModifier(float value)
    {
        FloatStat result = new FloatStat(value, internalUpdate);
        stats.Add(result);
        internalUpdate();
        return result;
    }
    public void removeModifier(FloatStat modifier)
    {
        stats.Remove(modifier);
        internalUpdate();
    }

    public static implicit operator float(FloatStatTracker t) //the style guides say this should be explict, not implicit. I can't see why
    {
        return t.value;
    }
}

public class FloatStat
{

    private float _value;
    public float value { get { return _value; } set { _value = value; update(); } }
    private FloatStatTracker.Recalculate update;
    public FloatStat(float val, FloatStatTracker.Recalculate update)
    {
        _value = val;
        this.update = update;
    }
    public static implicit operator float(FloatStat stat)
    {
        return stat.value;
    }
}


