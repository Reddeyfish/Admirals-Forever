using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

[RequireComponent(typeof(AudioSource))] // just in case someone really screws up when importing
[RequireComponent(typeof(VolumeController))]
public class MusicManager : MonoBehaviour
{
    protected AudioSource source;
    protected static MusicManager _self; //there can only be one
    public static MusicManager self { get { return _self; } }

    protected virtual void Awake()
    {
        if (_self != null)
            Destroy(_self.gameObject);
        DontDestroyOnLoad(this.gameObject);
        source = GetComponent<AudioSource>();
        _self = this;
        Callback.FireAndForget(() => Destroy(this.gameObject), source.clip.length, this, mode: Callback.Mode.REALTIME);
    }
}
