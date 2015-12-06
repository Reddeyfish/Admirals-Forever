using UnityEngine;
using System.Collections;

public class ParticlesObeyRotation : MonoBehaviour {

    ParticleSystem particles;
	// Use this for initialization
	void Awake () {
        particles = GetComponent<ParticleSystem>();
	}

    public void DoUpdate()
    {
        particles.startRotation = Mathf.Deg2Rad * (transform.rotation.eulerAngles.z);
    }
}
