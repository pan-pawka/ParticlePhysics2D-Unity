/// <summary>
/// Springs connect 2 particles and try to keep them a certain distance apart. They have 3 properties:
/// Rest Length - the spring wants to be at this length and acts on the particles to push or pull them exactly this far apart at all times.
///	Strength - If they are strong they act like a stick. If they are weak they take a long time to return to their rest length. ie Spring Constant
///	Damping - If springs have high damping they don't overshoot and they settle down quickly, with low damping springs oscillate.
/// </summary>

using UnityEngine;
using System.Collections;

namespace ParticlePhysics2D {
	public class Spring2D : IForce {
		float springConstant;
		float damping;
		float restLength;
		Particle2D a, b;
		bool on;
		
		public Spring2D( Particle2D A, Particle2D B, float springConstant, float damping, float restLength )
		{
			this.springConstant = springConstant;
			this.damping = damping;
			this.restLength = restLength;
			a = A;
			b = B;
			on = true;
		}
		
		public void turnOff()
		{
			on = false;
		}
		
		public void turnOn()
		{
			on = true;
		}
		
		public bool isOn()
		{
			return on;
		}
		
		public bool isOff()
		{
			return !on;
		}
		
		public Particle2D getOneEnd()
		{
			return a;
		}
		
		public Particle2D getTheOtherEnd()
		{
			return b;
		}
		
		public float currentLength()
		{
			return (a.Position-b.Position).magnitude;
		}
		
		public float RestLength
		{
			get {return restLength;}
			set {restLength = value;}
		}
		
		public float strength()
		{
			return springConstant;
		}
		
		public void setStrength( float ks )
		{
			springConstant = ks;
		}
		
		public float Damping
		{
			get{return damping;}
		}
		
		public void setDamping( float d )
		{
			damping = d;
		}
		
		public void setRestLength( float l )
		{
			restLength = l;
		}
		
		public void apply()
		{	
			if ( on && ( a.IsFree || b.IsFree ) )
			{
				float a2bX = a.Position.x - b.Position.x;
				float a2bY = a.Position.y - b.Position.y;
				
				float a2bDistance = (float)Mathf.Sqrt( a2bX*a2bX + a2bY*a2bY  );
				
				if ( a2bDistance == 0 )
				{
					a2bX = 0;
					a2bY = 0;
				}
				else
				{
					a2bX /= a2bDistance;
					a2bY /= a2bDistance;
				}
				
				
				// spring force is proportional to how much it stretched 
				
				float springForce = -( a2bDistance - restLength ) * springConstant; 
				
				
				// want velocity along line b/w a & b, damping force is proportional to this
				
				float Va2bX = a.Velocity.x - b.Velocity.x;
				float Va2bY = a.Velocity.y - b.Velocity.y;
				
				float dampingForce = -damping * ( a2bX*Va2bX + a2bY*Va2bY );
				
				
				// forceB is same as forceA in opposite direction
				
				float r = springForce + dampingForce;
				
				a2bX *= r;
				a2bY *= r;
				
				if ( a.IsFree )
					a.Force += new Vector2 (a2bX,a2bY);
				if ( b.IsFree )
					b.Force -= new Vector2 (a2bX,a2bY);
			}
		}
		
		void setA( Particle2D p )
		{
			a = p;
		}
		
		void setB( Particle2D p )
		{
			b = p;
		}
	
	}
}
