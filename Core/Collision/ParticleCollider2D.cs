//Yves Wang @ FISH, 2015, All rights reserved
//a particle collider 2d is basically a circular collider which specifically colliders with the collision holder

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {
	
	[RequireComponent(typeof(Collider2D))]
	public sealed class ParticleCollider2D : CollisionObject  {
		
		void NarrowPhaseUpdate() {
			
		}
		
		protected override void Start() {
			base.Start();
			this.UpdateMethod += NarrowPhaseUpdate;
			this.phaseType = PhaseType.Narrow;
		}
		
		
		
	}
	
}