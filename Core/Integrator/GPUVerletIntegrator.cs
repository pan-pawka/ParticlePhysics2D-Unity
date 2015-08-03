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
		
		public GPUVerletIntegrator (Simulation sim) {
			
		}
		
		public void step(float dt) {
			
		}
	}
	
}
