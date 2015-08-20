//Yves Wang @ FISH, 2015, All rights reserved

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {

	[AddComponentMenu("ParticlePhysics2D/Collision/Manager",13)]
	public class PCollision2DManager : Singleton<PCollision2DManager> {
		
		//singleton instance
		public static PCollision2DManager Instance {
			get { return ((PCollision2DManager)mInstance);} 
			set { mInstance = value;}
		}
		
		//static ctor
		static PCollision2DManager () {
			Debug.Log("Collision Manager created : " + PCollision2DManager.Instance.name);
		}
		
		public const bool IsDebugOn = true;
		[SerializeField] CollisionProcessor bpProcessor = new CollisionProcessor (IsDebugOn);	//broad phase processor
		[SerializeField] CollisionProcessor npProcessor = new CollisionProcessor (IsDebugOn);  //narrow phase processor

		//generic add and remove
		public void AddCollisionObject ( CollisionObject obj ) {
			if (obj.phaseType == PhaseType.Broad) bpProcessor.AddObject(obj); else npProcessor.AddObject(obj);
		}
		
		public void RemoveCollisionObject ( CollisionObject obj) {
			if (obj.phaseType == PhaseType.Broad) bpProcessor.RemoveObject(obj); else npProcessor.RemoveObject(obj);
		}
		
		
		void FixedUpdate () {
			bpProcessor.Update(Time.deltaTime);
			npProcessor.Update(Time.deltaTime);
		}
	}
}

