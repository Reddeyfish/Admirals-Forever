using UnityEngine;
using System.Collections;

//despawns the host gameobject after it's audiosource finishes playing. Intended for prefab-spawned effects

[RequireComponent(typeof(AudioSource))]
public class DespawnOnAudioFinish : MonoBehaviour {

    AudioSource sound;

    void Awake()
    {
        sound = GetComponent<AudioSource>();
    }
	// Use this for initialization
    void OnEnable()
    {
        StartCoroutine(MainRoutine());
    }

    IEnumerator MainRoutine()
    {
        yield return new WaitForSeconds(sound.clip.length);
        SimplePool.Despawn(this.gameObject);
    }
}
