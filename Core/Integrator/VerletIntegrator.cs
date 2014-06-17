using UnityEngine;
using System.Collections;

namespace ParticlePhysics2D {
	public class VerletIntegrator : IIntegrator {
		
		Simulation s;
		
		public VerletIntegrator( Simulation s )
		{
			this.s = s;
		}
		
		public void step( float t )
		{
			s.clearForces();
			s.applyForces();
			
			Vector2 temp;
			for ( int i = 0; i < s.numberOfParticles(); i++ )
			{
				Particle2D p = s.getParticle(i);
				if ( p.IsFree )
				{
					temp = p.Position;
					p.Position = p.Position + (p.Position - p.PositionOld);
					p.PositionOld = temp;
				}
			}
		}
		
	}

}
