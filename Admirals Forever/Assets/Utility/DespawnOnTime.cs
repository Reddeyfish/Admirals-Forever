using UnityEngine;
using System.Collections;

//despawns the host gameobject after a set time

public class DespawnOnTime : MonoBehaviour {

    public float time;

    void OnEnable()
    {
        StartCoroutine(MainRoutine());
    }

    IEnumerator MainRoutine()
    {
        yield return new WaitForSeconds(time);
        SimplePool.Despawn(this.gameObject);
    }
}
