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
		static Material springMtl,angleMtl,verletMtl;
		static Material springDeltaMtl,angleDeltaMtl;
		
		YieldInstruction eof;
		
		public GPUVerletIntegrator (Simulation sim) : base(sim) {
			//remember to add these shader in the pre loaded assets
			if (springMtl==null) springMtl = new Material (Shader.Find("ParticlePhysics2D/SpringConstraint"));
			if (angleMtl==null) angleMtl = new Material (Shader.Find("ParticlePhysics2D/AngleConstraint"));
			if (verletMtl==null) verletMtl = new Material (Shader.Find("ParticlePhysics2D/VerletGPUIntegrator"));
			if (springDeltaMtl==null) springDeltaMtl = new Material (Shader.Find("ParticlePhysics2D/SpringDelta"));
			if (angleDeltaMtl==null) angleDeltaMtl = new Material (Shader.Find("ParticlePhysics2D/AngleDelta"));
//			if (springMtl==null) Debug.LogError("SpringMtl null");
//			if (angleMtl==null) Debug.LogError("angleMtl null");
//			if (verletMtl==null) Debug.LogError("verletMtl null");
//			if (springDeltaMtl==null) Debug.LogError("springDeltaMtl null");
//			if (angleDeltaMtl==null) Debug.LogError("angleDeltaMtl null");
			simbuffer = SimBuffer.Create(sim);
			eof = new WaitForEndOfFrame();
		}
		
		protected sealed override void StepMethod(){
			
			//SimulationManager.Instance.StopCoroutine(GPUStep());
			SimulationManager.Instance.StartCoroutine(GPUStep());
			
		}
		
		IEnumerator GPUStep () {
		
			//get the position of particles to PositionRT
			simbuffer.SendToGPU_ParticlePosition();
			
			//this step is disabled to debug
			simbuffer.Update(springMtl,springDeltaMtl, angleMtl,angleDeltaMtl, verletMtl);
		
			//wait till the end of the frame, then read RT into particle Position list,i.e., from gpu to cpu
			yield return eof;
			
			//get PositionRT 's data into particles position
			simbuffer.SendToCPU_ParticlePosition();
			
		}
		
		
	}
	
	
	
}
