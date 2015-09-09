/// Yves Wang @ FISH, 2015, All rights reserved
/// <summary>
/// GPU verlet integrator perform verlet integration on GPU by using vertex-frag shader pipeline.
/// if using GPU Verlet, all calculations including force application, are performed on GPU
/// therefore, ParticePhysics2D.cs is only the data holder.
/// ----
/// Basically, a integrator applys constraints and calculate the new position based on the previous one.
/// And because for now, collision detection must be done in CPU, so in each GPU integrator step,
/// we have to fetch all the particle into a texture, and in the end of each step, we take the new position from texture
/// to the generic list where all the particles store their information.
/// </summary>

using UnityEngine;
using System.Collections;
using ParticlePhysics2D;

namespace ParticlePhysics2D {
	public class GPUVerletIntegrator : IntegratorBase {
		
		SimBuffer simbuffer;
		public Shader VerletSpringConstraint,VerletAngleConstraint,VerletGPUIntegrator;
		private static Material springMtl,angleMtl,verletMtl;
		
		public GPUVerletIntegrator (Simulation sim) : base(sim) {
			springMtl = new Material (VerletSpringConstraint);
			angleMtl = new Material (VerletAngleConstraint);
			verletMtl = new Material (VerletGPUIntegrator);
			simbuffer = SimBuffer.Create(sim);
		}
		
		protected sealed override void StepMethod(){
			
			SimulationManager.Instance.StopCoroutine(GPUStep());
			SimulationManager.Instance.StartCoroutine(GPUStep());
			
		}
		
		IEnumerator GPUStep () {
		
			//get back all the data from cpu to gpu
			simbuffer.SendToGPU_ParticlePosition();
			
			simbuffer.Update(springMtl,angleMtl,verletMtl);
		
			//wait till the end of the frame, then read RT into particle Position list,i.e., from gpu to cpu
			yield return new WaitForEndOfFrame();
			
			simbuffer.SendToCPU_ParticlePosition();
			
		}
		
		
	}
	
	
	
}
