//Yves Wang @ FISH, 2015, All rights reserved
// the base class for all 2d collision, which is the collsion holder
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {
	
	[RequireComponent(typeof(CircleCollider2D))]
	public class ParticleCollision2D : MonoBehaviour {
	
		public float lastUpdateTime;
	
		protected List<ParticleCollider2D> pCollider = new List<ParticleCollider2D> ();

		protected virtual void Start () {
			this.GetComponent<CircleCollider2D>().isTrigger = true;
			lastUpdateTime = Time.time;
		}
			
		protected virtual void Update () {}
		
		
		protected virtual void LateUpdate () {
			
		}
		
		void OnTriggerEnter2D (Collider2D c) {
			ParticleCollider2D pC2D = c.gameObject.GetComponent<ParticleCollider2D>();
			if (pC2D) {
				if (!pCollider.Contains(pC2D)) {
					//if no previous collider is added
					if (pCollider.Count==0) {
						PCollision2DManager.collisionHolders.Add(this);
					}
					pCollider.Add(pC2D); //particle collider 2d is being added into particle collision holder
					//
				}
			}
		}
		
		void OnTriggerExit2D (Collider2D c) {
			ParticleCollider2D pC2D = c.gameObject.GetComponent<ParticleCollider2D>();
			if (pC2D) {
				if (pCollider.Contains(pC2D)) {
					pCollider.Remove(pC2D);//particle collider 2d is leaving from the collision holder
					if (pCollider.Count==0) {
						PCollision2DManager.collisionHolders.Remove(this);
					}
					//
				}
			}
		}
	}
}

