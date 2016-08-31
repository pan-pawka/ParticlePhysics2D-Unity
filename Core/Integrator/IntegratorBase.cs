//Yves Wang @ FISH, 2015, All rights reserved
using UnityEngine;
using System.Collections;

namespace ParticlePhysics2D {

	public abstract class IntegratorBase  {
	
		protected Simulation sim;
		protected SimSettings setting;
		float lastUpdateTime;
		
		protected IntegratorBase(Simulation sim) {
			this.sim = sim;
			this.lastUpdateTime = new System.Random().Next(10000) / 10000f + Time.realtimeSinceStartup;
			this.setting = sim.Settings;
		}
		
		protected abstract void StepMethod();
		
		public void step() {
			StepMethod();
//			Using a fixed time step to do verlet update
//			float timeNow = Time.realtimeSinceStartup;
//			while (timeNow - this.lastUpdateTime > SimulationManager.Instance.FixedTimestep_Verlet) {
//				StepMethod();
//				this.lastUpdateTime += SimulationManager.Instance.FixedTimestep_Verlet;
//			}
		}
		
	}
	
	

}

