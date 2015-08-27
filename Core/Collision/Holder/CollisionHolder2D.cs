//Yves Wang @ FISH, 2015, All rights reserved
// the base class for all 2d collision, which is the collsion holder
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {
	
	[RequireComponent(typeof(CircleCollider2D))]
	public abstract class CollisionHolder2D : CollisionObject {
		
		const int DEFAULT_CONNECTION_NUM = 10;
		
		public float leafRadius = 1f;
		
		/// <summary>
		/// The circle collider is working as a trigger, toggling the collsion globally
		/// </summary>
		protected CircleCollider2D circle;
		
		[HideInInspector]
		public List<CollisionTarget2D> connection = new List<CollisionTarget2D> (DEFAULT_CONNECTION_NUM);
		
		//derived class must override and extend the start
		protected virtual void Start () {
			base.UpdateMethod += BroadPhaseUpdate;
			circle = this.GetComponent<CircleCollider2D>();
			circle.isTrigger = true;
		}
		
		//derived class must override this method
		//the broad phase implementation includes calculating the BVH
		protected abstract void BroadPhaseUpdate();
		
		public abstract void TraverseBVHForCircle ( CircleCollider2D circle,out int searchCount ) ;
		public abstract void TraverseBVHForPolygon ( PolygonCollider2D poly ) ;
			
		protected virtual void Update () {}
		
		protected virtual void LateUpdate () {}
		
		protected virtual void OnTriggerEnter2D (Collider2D c) {
			if (!this.enabled) return;
			CollisionTarget2D pC2D = c.gameObject.GetComponent<CollisionTarget2D>();
			if (pC2D) {
				CollisionObject.Connect(this,pC2D);
			}
		}
		
		protected virtual void OnTriggerExit2D (Collider2D c) {
			if (!this.enabled) return;
			CollisionTarget2D pC2D = c.gameObject.GetComponent<CollisionTarget2D>();
			if (pC2D) {
				CollisionObject.Disconnect(this,pC2D);
			}
		}
		//
		public bool debugTrigger = false;
		protected virtual void OnDrawGizmos() {
			if (debugTrigger==true) {
				if (!circle) circle = this.GetComponent<CircleCollider2D>();
				Vector2 center = transform.localToWorldMatrix.MultiplyPoint3x4(circle.offset);
				DebugExtension.DrawCircle(center,Vector3.forward,Color.green * 0.5f,circle.radius);
			}
		}
		
		
	}
}

