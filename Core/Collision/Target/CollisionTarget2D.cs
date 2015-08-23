//Yves Wang @ FISH, 2015, All rights reserved
//this is a base class for all the collider2D which are able to collide with CollisionHolder2D

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {
	
	[RequireComponent(typeof(Collider2D),typeof(Rigidbody2D))]
	public abstract class CollisionTarget2D : CollisionObject  {
		
		const int DEFAULT_CONNECTION_NUM = 10;
		
		private Rigidbody2D rb2d;
		public Rigidbody2D RigidBody2D {get{return rb2d;}}
		
		[HideInInspector]
		public List<CollisionHolder2D> connection = new List<CollisionHolder2D> (DEFAULT_CONNECTION_NUM);
		
		protected abstract void NarrowPhaseUpdate();
		
		protected virtual void Start() {
			this.rb2d = this.GetComponent<Rigidbody2D>();
			base.UpdateMethod += NarrowPhaseUpdate;
		}
	}
	
}