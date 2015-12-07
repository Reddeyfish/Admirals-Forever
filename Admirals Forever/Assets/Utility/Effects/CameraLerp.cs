using UnityEngine;
using System.Collections;

//moves it's Rigidbody2D to follow the player's transform

[RequireComponent(typeof(Rigidbody2D))]
public class FollowLerp : MonoBehaviour {
    public float smoothTime = 1f;

    private Transform player;
    private Rigidbody2D rigid;
	// Use this for initialization
	void Start () {
        rigid = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag(Tags.player).transform;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector3 velocity = Vector3.zero;
        transform.position = Vector3.SmoothDamp(transform.position, player.position, ref velocity, smoothTime);
        rigid.velocity = velocity;
	}
}
