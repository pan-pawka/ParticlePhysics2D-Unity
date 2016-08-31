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
			if ( setting.gravity != Vector2.zero )
			{
				for ( int i = 0; i < sim.numberOfParticles(); ++i )
				{
					p = sim.getParticle(i);
					if (p.IsFree) p.Position += setting.gravity;
				}
			}
			
			//iterations
			for (int iter=0;iter<setting.iteration;iter++) {
				if (setting.applyString) {
					for ( int p = 1 ; p<=sim.maxSpringConvergenceID ; p++ ) 
					for ( int i = 0; i < sim.numberOfSprings(); i++ ){
						Spring2D sp = sim.getSpring(i);
						if (sp.convergenceGroupID == p) 
							sp.apply();
					}
				}
				
				if (setting.applyAngle) {
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
					//This is a simplified version
					temp = p.Position;
					p.Position += (p.Position - p.PositionOld) * setting.damping;
					p.PositionOld = temp;

					/* Different versions
					temp = p.Position;
					//p.Position = p.Position + (p.Position - p.PositionOld);
					//p.Position = p.Position + (p.Position - p.PositionOld) * s.damping  * (t / dt) + p.Force / p.Mass * t * t; //With force,mass and time-corrected
					//p.Position = p.Position + (p.Position - p.PositionOld) * s.damping + p.Force;
					p.Position += (p.Position - p.PositionOld) * s.damping;
					p.PositionOld = temp;
					//dt = t;//dt is the previous deltaTime
					*/
				}
			}
		}
		

	}

}

