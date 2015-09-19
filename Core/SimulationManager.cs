//Yves Wang @ FISH, 2015, All rights reserved

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {

	[AddComponentMenu("ParticlePhysics2D/Simulation Manager",13)]
	public class SimulationManager : Singleton<SimulationManager> {
		
		//singleton instance
		public static SimulationManager Instance {
			get { return ((SimulationManager)mInstance);} 
			set { mInstance = value;}
		}
		
		//gpu materials
		[Header("GPU Integrator Mtl")]
		public Material verletMtl;
		public Material springMtl,angleMtl,springDeltaMtl,angleDeltaMtl;
		
		//global parameters
		[HideInInspector]
		public float FixedTimestep = 1f/30f;
		
		[Space(10f)]
		
		[Range(10,60)]
		public int UpdatePerSecond = 30;
		private int _updatePerSecond;
		
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
		
		void Start() {
			_updatePerSecond = this.UpdatePerSecond;
			this.FixedTimestep = 1f/this.UpdatePerSecond;
		}
		
		void Update() {
			if (UpdatePerSecond != _updatePerSecond) {
				_updatePerSecond = UpdatePerSecond;
				this.FixedTimestep = 1f/UpdatePerSecond;
			}
		}
		
		void FixedUpdate () {
			bpProcessor.Update(Time.deltaTime);
			npProcessor.Update(Time.deltaTime);
		}
	}
}

