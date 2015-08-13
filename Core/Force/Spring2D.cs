/// <summary>
/////Yves Wang @ FISH, 2015, All rights reserved
/// Springs connect 2 particles and try to keep them a certain distance apart. They have 3 properties:
/// Rest Length - the spring wants to be at this length and acts on the particles to push or pull them exactly this far apart at all times.
///	Strength - If they are strong they act like a stick. If they are weak they take a long time to return to their rest length. ie Spring Constant
///	Damping - If springs have high damping they don't overshoot and they settle down quickly, with low damping springs oscillate.
/// </summary>

using UnityEngine;
using System.Collections;

namespace ParticlePhysics2D {
	
	[System.Serializable]
	public class Spring2D : IForce {
		
		[SerializeField] float springConstant;
		[SerializeField] float restLength2;
		[SerializeField] Particle2D a;
		[SerializeField] Particle2D b;
		[SerializeField] bool on;
		
		public Spring2D( Particle2D A, Particle2D B, float springConstant, float restLength )
		{
			this.springConstant = springConstant;
			this.restLength2 = restLength * restLength;
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
		
		public Particle2D ParticleA 
		{
			get {return a;} set {a=value;}
		}
		
		public Particle2D ParticleB 
		{
			get {return b;} set {b=value;}
		}
		
		public float currentLength()
		{
			return (a.Position-b.Position).magnitude;
		}
		
		public float strength()
		{
			return springConstant;
		}
		
		public void setStrength( float ks )
		{
			springConstant = ks;
		}
		
		public void setRestLength( float l )
		{
			restLength2 = l * l;
		}
		
		public void apply()
		{	
			if ( on && ( a.IsFree || b.IsFree ) )
			{
				//faster square root approx from Advanced Character Physics
				Vector2 delta = a.Position - b.Position;
				delta *= restLength2 /(delta.sqrMagnitude + restLength2) - 0.5f;
				if (a.IsFree) a.Force -= delta * springConstant;
				if (a.IsFree) b.Force += delta * springConstant;
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
		
		static Color springColor = Color.cyan - new Color (0f,0f,0f,0.2f);
		public void DebugSpring(Matrix4x4 local2World){
			Vector2 aPos = local2World.MultiplyPoint3x4(a.Position);
			Vector2 bPos = local2World.MultiplyPoint3x4(b.Position);
			Debug.DrawLine(aPos,bPos,springColor);
			//if (a.IsLeaf) DebugExtension.DebugCircle(aPos,Vector3.forward,Color.red,0.1f);
			//if (b.IsLeaf) DebugExtension.DebugCircle(bPos,Vector3.forward,Color.red,0.1f);
		}
		public void DebugSpring(){
			Debug.DrawLine(a.Position,b.Position,springColor);
			if (a.IsLeaf) DebugExtension.DebugCircle(a.Position,Vector3.forward,Color.red,1f);
			if (b.IsLeaf) DebugExtension.DebugCircle(b.Position,Vector3.forward,Color.red,1f);
		}
	
	}
}
