//Yves Wang @ FISH, 2015, All rights reserved
// the base class for all 2d collision, which is the collsion holder
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {
	
	[RequireComponent(typeof(CircleCollider2D))]
	public abstract class CollisionHolder2D : CollisionObject {
		
		//derived class must override and extend the start
		protected override void Start () {
			base.Start();
			this.UpdateMethod += BroadPhaseUpdate;
			this.phaseType = PhaseType.Broad;
		}
		
		//derived class must override this method
		//the broad phase implementation includes calculating the BVH
		protected abstract void BroadPhaseUpdate();
			
		protected virtual void Update () {}
		
		protected virtual void LateUpdate () {}
		
		protected virtual void OnTriggerEnter2D (Collider2D c) {
			ParticleCollider2D pC2D = c.gameObject.GetComponent<ParticleCollider2D>();
			if (pC2D) {
				this.Connect(pC2D);
			}
		}
		
		protected virtual void OnTriggerExit2D (Collider2D c) {
			ParticleCollider2D pC2D = c.gameObject.GetComponent<ParticleCollider2D>();
			if (pC2D) {
				this.Disconnect(pC2D);
			}
		}
		
		
	}
}

