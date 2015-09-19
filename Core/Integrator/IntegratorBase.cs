//Yves Wang @ FISH, 2015, All rights reserved
using UnityEngine;
using System.Collections;

namespace ParticlePhysics2D {

	public abstract class IntegratorBase  {
	
		protected Simulation sim;
		float lastUpdateTime;
		//protected System.Action StepMethodDelegate;//set the stepmethod when implement this class
		
		protected IntegratorBase(Simulation sim) {
			this.sim = sim;
			this.lastUpdateTime = new System.Random().Next(10000) / 10000f + Time.realtimeSinceStartup;
		}
		
		protected abstract void StepMethod();
		
		public void step() {
			StepMethod();
//			float timeNow = Time.realtimeSinceStartup;
//			while (timeNow - this.lastUpdateTime > SimulationManager.Instance.FixedTimestep) {
//				StepMethod();
//				this.lastUpdateTime += SimulationManager.Instance.FixedTimestep;
//			}
		}
		
	}
	
	

}

