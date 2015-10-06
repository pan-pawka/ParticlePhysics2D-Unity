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
		public static Material springMtl,angleMtl,verletMtl;
		public static Material angleDeltaMtl;
		
		YieldInstruction eof;
		
		public GPUVerletIntegrator (Simulation sim) : base(sim) {
			//remember to add these shader in the pre loaded assets
			if (springMtl==null) springMtl = new Material ( Shader.Find("ParticlePhysics2D/Spring"));
			if (angleMtl==null) angleMtl = new Material ( Shader.Find("ParticlePhysics2D/Angle"));
			if (verletMtl==null) verletMtl = new Material ( Shader.Find("ParticlePhysics2D/VerletGPUIntegrator"));
			if (angleDeltaMtl==null) angleDeltaMtl = new Material ( Shader.Find("ParticlePhysics2D/AngleDelta"));
			simbuffer = SimBuffer.Create(sim);
			eof = new WaitForEndOfFrame();
		}
		
		protected sealed override void StepMethod(){
			//SimulationManager.Instance.StopCoroutine(GPUStep());
			SimulationManager.Instance.StartCoroutine(GPUStep());
		}
		
		IEnumerator GPUStep () {
		
			simbuffer.SendToGPU_ParticlePosition();//get the position of particles to PositionRT
			simbuffer.Update();//this step is disabled to debug
			yield return eof;//wait till the end of the frame, then read RT into particle Position list,i.e., from gpu to cpu
			simbuffer.SendToCPU_ParticlePosition();//get PositionRT 's data into particles position
			
		}
		
		
	}
	
	
	
}
