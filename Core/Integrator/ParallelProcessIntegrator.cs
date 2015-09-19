using UnityEngine;
using UnityEngine;
using System.Collections;
using CzlParallelFor;

namespace ParticlePhysics2D {
	
	public class ParallelProcessIntegrator : IntegratorBase {
		
		ParallelProcessManager pm;
		int springCount,angleCount;
		
		public ParallelProcessIntegrator( Simulation s ) : base(s)
		{
			pm = new ParallelProcessManager ();
			pm.maxThreadCount = 2 * System.Environment.ProcessorCount;
			springCount = sim.applySpring ? sim.numberOfSprings() : 0;
			angleCount = sim.applyAngle ? sim.numberOfAngleConstraints() : 0;
		}
		
		protected sealed override void StepMethod() {
			//gravity
			if ( base.sim.getGravity() != Vector2.zero )
			{
				for ( int i = 0; i < base.sim.numberOfParticles(); ++i )
				{
					var p = base.sim.getParticle(i);
					if (p.IsFree) p.Position += sim.getGravity();
				}
			}
			
			pm.updateAction = UpdateConstraint;
			pm.SetProcess(springCount + angleCount);
			for (int iter = 0;iter < sim.ITERATIONS * 2;iter ++ ) pm.Update();
			//pm.WaitUpdateEnd();
			pm.updateAction = verletParticle;
			pm.SetProcess(sim.numberOfParticles());
			pm.Update();
			//pm.WaitUpdateEnd();
		}
		
		void UpdateConstraint(int start,int count) {
			//return;
			int index = start;
			for (int i=0;i<count;i++) {
				if (index+1 <= springCount) sim.getSpring(index).applyThreaded();
				else sim.getAngleConstraint(index - springCount).applyThreaded();
				index++;
			}
		}
		
		void verletParticle(int start, int count) {
			//return;
			int index = start;
			for (int i=0;i<count;i++) {
				Particle2D pp = base.sim.getParticle(index);
				if (pp.IsFree) {
					//lock(pp) {
						Vector2 tempv2 = pp.Position;
						pp.Position += (pp.Position - pp.PositionOld) * base.sim.damping;
						pp.PositionOld = tempv2;
					//}
					
				}
				index++;
			}
		}
		
	}
	
}

