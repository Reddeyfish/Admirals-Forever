using UnityEngine;
using System.Collections;

public class MusicManagerOnEnd : MusicManager {
    protected override void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        source = GetComponent<AudioSource>();
        _self = this;
    }
}
