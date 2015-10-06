using UnityEngine;
using System.Collections;

namespace ParticlePhysics2D {

	public class MultiThreadedIntegrator : IntegratorBase {
		
		
		public MultiThreadedIntegrator( Simulation s ) : base(s)
		{
	
		}
		
		void verletParticle(int i) {
			Particle2D pp = base.sim.getParticle(i);
			if (pp.IsFree) {
				Vector2 tempv2 = pp.Position;
				pp.Position += (pp.Position - pp.PositionOld) * base.sim.Settings.damping;
				pp.PositionOld = tempv2;
			}
		}
		
		void spring (int i) {
			base.sim.getSpring(i).apply();
		}
		
		void angle( int i) {
			base.sim.getAngleConstraint(i).apply();
		}
		
		protected sealed override void StepMethod() {
			//gravity
			if ( base.sim.Settings.gravity != Vector2.zero )
			{
				for ( int i = 0; i < base.sim.numberOfParticles(); ++i )
				{
					var p = base.sim.getParticle(i);
					if (p.IsFree) p.Position += sim.Settings.gravity;
				}
			}
			
			for (int iter = 0;iter < sim.Settings.iteration * 2;iter ++ ) {
				//spring
				if (base.sim.Settings.applyString) Parallel.For(0,base.sim.numberOfSprings(),spring);
				//angle
				if (base.sim.Settings.applyAngle) Parallel.For(0,base.sim.numberOfAngleConstraints(),angle);
			}
			
			//verlet
			Parallel.For(0,base.sim.numberOfParticles(),verletParticle);
			
		}
	}
	
}

