//Yves Wang @ FISH, 2015, All rights reserved
//a particle collider 2d is basically a circular collider which specifically colliders with the collision holder

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {
	
	[RequireComponent(typeof(Collider2D))]
	public sealed class ParticleCollider2D : MonoBehaviour {
		
		public float radius = 5f;
		public float lastUpdateTime;
		List<CollisionHolder2D> collisionHolders = new List<CollisionHolder2D> (5);
		
		public void AttachToCollisionHolder(CollisionHolder2D c) {
			if (!collisionHolders.Contains(c)) collisionHolders.Add(c);
		}
		
		public void DettachFromCollisionHolder(CollisionHolder2D c) {
			if (collisionHolders.Contains(c)) collisionHolders.Remove(c);
		}
		
		public int CollisionHolderCount {
			get {return collisionHolders.Count;}
		}
		
		public int indexInManager = -1;
			
		
		public void NarrowPhaseUpdate() {
			
		}
		
		void Start() {
			this.GetComponent<Collider2D>().isTrigger = true;
		}
		
	}
	
}