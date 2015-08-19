using UnityEngine;
using System.Collections;
using System;

namespace ParticlePhysics2D {

	public abstract class CollisionObject : MonoBehaviour {
	
		public int indexInManager = -1;
		public float lastUpdateTime = 0f;
		public Action UpdateMethod {get;set;}//must set the update method
		
	}
}

