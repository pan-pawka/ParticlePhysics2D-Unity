/// <summary>
/// GPU verlet integrator perform verlet integration on GPU by using vertex-frag shader pipeline.
/// if using GPU Verlet, all calculations including force application, are perform on GPU
/// therefore, ParticePhysics2D.cs is only the data holder.
/// </summary>

using UnityEngine;
using System.Collections;
using ParticlePhysics2D;

namespace ParticlePhysics2D {
	public class GPUVerletIntegrator : IIntegrator {
		
		Simulation sim;
		SimBuffer simbuffer;
		
		public GPUVerletIntegrator (Simulation sim) {
			this.sim = sim;
			this.simbuffer = SimBuffer.Create(sim);
		}
		
		//this is called outside by some gpu collision solver,
		//to manipulate the position tex
		public void BlitCollision ( Material mtl,int pass = -1) {
			this.simbuffer.BlitPosition(mtl,pass);
		}
		
		public void step(float dt) {
			
		}
		
		
	}
	
	
	
}
