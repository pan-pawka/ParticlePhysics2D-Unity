//Yves Wang @ FISH, 2015, All rights reserved
//a particle collider 2d is basically a circular collider which specifically colliders with the collision holder

using UnityEngine;
using System.Collections;

namespace ParticlePhysics2D {
	
	[RequireComponent(typeof(Collider2D))]
	public sealed class ParticleCollider2D : MonoBehaviour {
		
		public float radius = 5f;
		public float lastUpdateTime;
		
	}
	
}