/// <summary>
/////Yves Wang @ FISH, 2015, All rights reserved
/// Springs connect 2 particles and try to keep them a certain distance apart. They have 3 properties:
/// Rest Length - the spring wants to be at this length and acts on the particles to push or pull them exactly this far apart at all times.
///	Strength - If they are strong they act like a stick. If they are weak they take a long time to return to their rest length. ie Spring Constant
///	Damping - If springs have high damping they don't overshoot and they settle down quickly, with low damping springs oscillate.
/// </summary>

using UnityEngine;
using System.Collections;
using System;

namespace ParticlePhysics2D {
	
	[System.Serializable]
	public class Spring2D : IForce {
		
		[SerializeField] float springConstant;
		[SerializeField] float restLength2;
		
		[SerializeField] int indexA;
		[SerializeField] int indexB;
		[SerializeField] bool on;
		
		[NonSerialized] Particle2D a;
		[NonSerialized] Particle2D b;
		
		// you need to SetSimulation in Simualtion class's OnAfterDeserialize() callback
		public void SetParticles(Simulation sim) {
			//this.sim = sim;
			a = sim.getParticle(indexA);
			b = sim.getParticle(indexB);
		}
		
		public Spring2D( Simulation sim, int indexA, int indexB, float springConstant, float restLength )
		{
			this.indexA = indexA;
			this.indexB = indexB;
			SetParticles(sim);
			this.springConstant = springConstant;
			this.restLength2 = restLength * restLength;
			on = true;
		}
		
		public Spring2D (Simulation sim, Particle2D a, Particle2D b, float springConstant, float restLength ) : 
			this (sim,sim.getParticleIndex(a),sim.getParticleIndex(b),springConstant,restLength )
		{
			
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
			//return (sim.getParticlePosition(indexA) - sim.getParticlePosition(indexB)).magnitude;
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
		
		static Color springColor = Color.cyan - new Color (0f,0f,0f,0.5f);
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
