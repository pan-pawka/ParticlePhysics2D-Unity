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
		public void AddCollisionObject ( CollisionHolder2D obj ) {
			bpProcessor.AddObject(obj);
		}
		
		public void AddCollisionObject( CollisionTarget2D obj) {
			npProcessor.AddObject(obj);
		}
		
		public void RemoveCollisionObject ( CollisionHolder2D obj) {
			bpProcessor.RemoveObject(obj); 
		}
		
		public void RemoveCollisionObject ( CollisionTarget2D obj) {
			npProcessor.RemoveObject(obj);
		}
		
		
		void FixedUpdate () {
			bpProcessor.Update(Time.deltaTime);
			npProcessor.Update(Time.deltaTime);
		}
	}
}

