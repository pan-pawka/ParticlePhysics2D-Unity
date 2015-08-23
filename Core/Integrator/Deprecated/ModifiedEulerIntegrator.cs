//using UnityEngine;
//using System.Collections;
//
//namespace ParticlePhysics2D {
//	
//	public class ModifiedEulerIntegrator  {
//		
//		Simulation s;
//		
//		public ModifiedEulerIntegrator( Simulation s )
//		{
//			this.s = s;
//		}
//		
//		public void step( float t )
//		{
//			s.clearForces();
//			s.applyForces();
//			
//			float halftt = 0.5f*t*t;
//			
//			for ( int i = 0; i < s.numberOfParticles(); i++ )
//			{
//				Particle2D p = s.getParticle(i);
//				if ( p.IsFree )
//				{
//					Vector2 a = p.Force / p.Mass;
//					
//					p.Position += p.Velocity / t;
//					p.Position += a * halftt;
//					p.Velocity += a / t;
//				}
//			}
//		}
//	}
//}
