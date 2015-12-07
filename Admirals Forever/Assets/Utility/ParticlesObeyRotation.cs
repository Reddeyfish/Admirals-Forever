using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class ParticlesObeyRotation : MonoBehaviour {

    ParticleSystem particles;
    public ParticleSystem ParticleSystem { get { return particles; } }
	// Use this for initialization
	void Awake () {
        particles = GetComponent<ParticleSystem>();
	}

    public void DoUpdate()
    {
        particles.startRotation = -Mathf.Deg2Rad * (transform.rotation.eulerAngles.z);
    }
}
