//Yves Wang @ FISH, 2015, All rights reserved
//Traditional Verlet : xi+1 = xi + (xi - xi-1) + a * dt * dt
//Timt-Corrected Verlet :  xi+1 = xi + (xi - xi-1) * (dti / dti-1) + a * dti * dti
//p.Position = p.Position + (p.Position - p.PositionOld) * s.damping  * (t / dt) + p.Force / p.Mass * t * t;

using UnityEngine;
using System.Collections;


namespace ParticlePhysics2D {

	public class VerletIntegrator : IntegratorBase {
	
		Particle2D p;//temp variable
		Vector2 temp;
		
		public VerletIntegrator( Simulation s ) : base(s)
		{
			//base.StepMethodDelegate = this.StepMethod;
		}
		
		protected sealed override void StepMethod() {
			this.applyConstraints();
			this.verlet();
		}
		
		
		void applyConstraints() {

		
			//gravity
			if ( sim.getGravity() != Vector2.zero )
			{
				for ( int i = 0; i < sim.numberOfParticles(); ++i )
				{
					p = sim.getParticle(i);
					if (p.IsFree) p.Position += sim.getGravity();
				}
			}
			
			//iterations
			for (int iter=0;iter<sim.ITERATIONS;iter++) {
				if (sim.applySpring) {
					for ( int p = 1 ; p<=sim.maxSpringConvergenceID ; p++ ) 
					for ( int i = 0; i < sim.numberOfSprings(); i++ ){
						Spring2D sp = sim.getSpring(i);
						if (sp.convergenceGroupID == p) 
							sp.apply();
					}
				}
				
				if (sim.applyAngle) {
					for ( int p = 1 ; p<=sim.maxAngleConvergenceID ; p++ ) {
						for ( int i = 0; i < sim.numberOfAngleConstraints(); i++ ){
							AngleConstraint2D ag = sim.getAngleConstraint(i);
							if (ag.convergenceGroupID == p) 
								ag.GetDelta();
						}
						
						for ( int i = 0; i < sim.numberOfAngleConstraints(); i++ ){
							AngleConstraint2D ag = sim.getAngleConstraint(i);
							if (ag.convergenceGroupID == p) 
								ag.apply();
						}
					}
					
				}
			}
		}
		
//		Original Verlet:
//		xi+1 = xi + (xi - xi-1) + a * dt * dt
//		Time-Corrected Verlet:
//		xi+1 = xi + (xi - xi-1) * (dti / dti-1) + a * dti * dti
		void verlet() {
			
			for ( int i = 0; i < base.sim.numberOfParticles(); i++ )
			{
				p = base.sim.getParticle(i);
				if ( p.IsFree )
				{
					temp = p.Position;
					p.Position += (p.Position - p.PositionOld) * base.sim.damping;
					p.PositionOld = temp;
					
				}
			}
		}
		

	}

}

