//Yves Wang @ FISH, 2015, All rights reserved

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {

	//[AddComponentMenu("ParticlePhysics2D/Collision/Manager",13)]
	public class SimulationManager : Singleton<SimulationManager> {
		
		//singleton instance
		public static SimulationManager Instance {
			get { return ((SimulationManager)mInstance);} 
			set { mInstance = value;}
		}
		
		//static ctor
		static SimulationManager () {
			Debug.Log("Collision Manager created : " + SimulationManager.Instance.name);
		}
		
		//global parameters
		public float FixedTimestep = 1f/30f;
		
		public bool IsDebugOn = false;
		CollisionProcessor bpProcessor = new CollisionProcessor ();	//broad phase processor
		CollisionProcessor npProcessor = new CollisionProcessor ();  //narrow phase processor
		
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

