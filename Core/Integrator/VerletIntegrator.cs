//Yves Wang @ FISH, 2015, All rights reserved
using UnityEngine;
using System.Collections;

namespace ParticlePhysics2D {

	public class VerletIntegrator : IIntegrator {
		
		Simulation s;
		float dt = 1f;
		
		public VerletIntegrator( Simulation s )
		{
			this.s = s;
		}
		
		public void step( float t )
		{
			
			s.applyConstraints();
			Vector2 temp;
			for ( int i = 0; i < s.numberOfParticles(); i++ )
			{
				Particle2D p = s.getParticle(i);
				if ( p.IsFree )
				{
					temp = p.Position;
					//p.Position = p.Position + (p.Position - p.PositionOld);
					//p.Position = p.Position + (p.Position - p.PositionOld) * s.damping  * (t / dt) + p.Force / p.Mass * t * t;
					//p.Position = p.Position + (p.Position - p.PositionOld) * s.damping + p.Force;
					p.Position += (p.Position - p.PositionOld) * s.damping;
					p.PositionOld = temp;
					//dt = t;
				}
			}
		}
	}

}

//Traditional Verlet : xi+1 = xi + (xi - xi-1) + a * dt * dt
//Timt-Corrected Verlet :  xi+1 = xi + (xi - xi-1) * (dti / dti-1) + a * dti * dti