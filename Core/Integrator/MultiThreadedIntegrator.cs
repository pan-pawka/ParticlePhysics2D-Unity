using UnityEngine;
using System.Collections;

namespace ParticlePhysics2D {

	public class MultiThreadedIntegrator : IntegratorBase {
		
		System.Action<int> task;
		
		public MultiThreadedIntegrator( Simulation s ) : base(s)
		{
			//base.StepMethodDelegate = this.StepMethod;
			this.task = this.verletParticle;
		}
		
		void verletParticle(int i) {
			Particle2D pp = base.sim.getParticle(i);
			if (pp.IsFree) {
				Vector2 tempv2 = pp.Position;
				pp.Position += (pp.Position - pp.PositionOld) * base.sim.damping;
				pp.PositionOld = tempv2;
			}
		}
		
		protected sealed override void StepMethod() {
			
		}
	}
	
}

