//Yves Wang @ FISH, 2015, All rights reserved
// the base class for all 2d collision, which is the collsion holder
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {
	
	[RequireComponent(typeof(CircleCollider2D))]
	public class CollisionHolder2D : MonoBehaviour {
	
		public float lastUpdateTime;
		private CircleCollider2D circle;
	
		protected List<ParticleCollider2D> pCollider = new List<ParticleCollider2D> ();
		
		protected virtual void Start () {
			this.GetComponent<CircleCollider2D>().isTrigger = true;
			lastUpdateTime = Time.time;
		}
		
		//derived class must override this method
		public virtual void BroadPhaseUpdate() {}
			
		protected virtual void Update () {}
		
		
		protected virtual void LateUpdate () {
			
		}
		
		protected virtual void OnTriggerEnter2D (Collider2D c) {
			//Debug.Log("DDD");
			ParticleCollider2D pC2D = c.gameObject.GetComponent<ParticleCollider2D>();
			if (pC2D) {
				if (!pCollider.Contains(pC2D)) {
					//if no previous collider is added
					if (pCollider.Count==0) {
						PCollision2DManager.AddCollisionHolder(this);
					}
					pC2D.AttachToCollisionHolder(this);
					pCollider.Add(pC2D); //particle collider 2d is being added into particle collision holder
					if (pC2D.indexInManager==-1) PCollision2DManager.AddPCollider2D(pC2D);
				}
			}
		}
		
		protected virtual void OnTriggerExit2D (Collider2D c) {
			Debug.Log("LLLL");
			ParticleCollider2D pC2D = c.gameObject.GetComponent<ParticleCollider2D>();
			if (pC2D) {
				if (pCollider.Contains(pC2D)) {
					pCollider.Remove(pC2D);//particle collider 2d is leaving from the collision holder
					pC2D.DettachFromCollisionHolder(this);
					if (pC2D.CollisionHolderCount==0) PCollision2DManager.RemovePCollider2D(pC2D);
					if (pCollider.Count==0) {
						PCollision2DManager.RemoveCollisionHolder(this);
					}
				}
			}
		}
		
		protected virtual void OnDrawGizmos() {
			if (!circle) circle = this.GetComponent<CircleCollider2D>();
			Vector2 center = transform.localToWorldMatrix.MultiplyPoint3x4(circle.offset);
			DebugExtension.DrawCircle(center,Vector3.forward,Color.green,circle.radius);
			
		}
	}
}

