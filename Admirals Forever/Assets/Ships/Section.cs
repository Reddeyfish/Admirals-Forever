using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class Section : MonoBehaviour {
    [SerializeField]
    protected float maxHealth;

    float health;
    Ship myShip;

    public int Side { get { return myShip.Side; } }
	// Use this for initialization
	void Awake () {
        health = maxHealth;
	}

    void Start()
    {
        myShip = GetComponentInParent<Ship>();
    }

    void OnCollisionEnter(Collision other)
    {
        Debug.Log("Ding");
    }

    public void takeHit(float damage)
    {
        health -= damage;
    }
}
