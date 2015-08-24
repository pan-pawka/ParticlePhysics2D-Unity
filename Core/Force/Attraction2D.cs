//using UnityEngine;
//using System.Collections;
//
//namespace ParticlePhysics2D {
//	public class Attraction2D : IForce {
//	
//		Particle2D a;
//		Particle2D b;
//		float k;
//		bool on;
//		float distanceMin;
//		float distanceMinSquared;
//		
//		public Attraction2D( Particle2D a, Particle2D b, float k, float distanceMin )
//		{
//			this.a = a;
//			this.b = b;
//			this.k = k;
//			on = true;
//			this.distanceMin = distanceMin;
//			this.distanceMinSquared = distanceMin*distanceMin;
//		}
//		
//		void setA( Particle2D p )
//		{
//			a = p;
//		}
//		
//		void setB( Particle2D p )
//		{
//			b = p;
//		}
//		
//		public float getMinimumDistance()
//		{
//			return distanceMin;
//		}
//		
//		public void setMinimumDistance( float d )
//		{
//			distanceMin = d;
//			distanceMinSquared = d*d;
//		}
//		
//		public void turnOff()
//		{
//			on = false;
//		}
//		
//		public void turnOn()
//		{
//			on = true;
//		}
//		
//		public void setStrength( float k )
//		{
//			this.k = k;
//		}
//		
//		public Particle2D getOneEnd()
//		{
//			return a;
//		}
//		
//		public Particle2D getTheOtherEnd()
//		{
//			return b;
//		}
//		
//		public void apply()
//		{
//			if ( on && ( a.IsFree || b.IsFree ) )
//			{
//				float a2bX = a.Position.x - b.Position.x;
//				float a2bY = a.Position.y - b.Position.y;
//				
//				float a2bDistanceSquared = a2bX*a2bX + a2bY*a2bY;
//				
//				if ( a2bDistanceSquared < distanceMinSquared )
//					a2bDistanceSquared = distanceMinSquared;
//				
//				float force = k * a.Mass * b.Mass / a2bDistanceSquared;
//				
//				float length = Mathf.Sqrt( a2bDistanceSquared );
//				
//				// make unit vector
//				
//				a2bX /= length;
//				a2bY /= length;
//				
//				// multiply by force 
//				
//				a2bX *= force;
//				a2bY *= force;
//				
//				// apply
//				
//				if ( a.IsFree )
//					a.Force -= new Vector2 (a2bX,a2bY);
//				if ( b.IsFree )
//					b.Force += new Vector2 (a2bX,a2bY);
//			}
//		}
//		
//		public float getStrength()
//		{
//			return k;
//		}
//		
//		public bool isOn()
//		{
//			return on;
//		}
//		
//		public bool isOff()
//		{
//			return !on;
//		}
//
//	}
//}