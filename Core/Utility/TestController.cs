using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CircleCollider2D),typeof(Rigidbody2D))]
public class TestController : MonoBehaviour {
	
	Rigidbody2D rb;
	public byte force;
	
	void Start() {
		rb = this.GetComponent<Rigidbody2D>();
	}
	
	void FixedUpdate () {
		Vector2 forceV = new Vector2 ( Input.GetAxis("Horizontal") , Input.GetAxis("Vertical")) * force;
		rb.AddForce(forceV);
	}
}
