using UnityEngine;
using System.Collections;
using ParticlePhysics2D;
using System;

namespace ParticlePhysics2D {

	public enum AngleConstraintTye { Rotation, Spring};

	[System.Serializable]
	public class AngleConstraint2D : IForce {
		
		//only b rotates around m
		[NonSerialized] Particle2D particleA, particleM, particleB;
		[SerializeField] int indexA,indexM,indexB;
		
		//[SerializeField] 
		[SerializeField] public float angle_Fixed; //constraint to this angle
		float angle_Cur; // how much the angle is currently
		
		[SerializeField] bool on;
		
		[NonSerialized] Simulation sim;
		
		/// <summary>
		/// The convergence group ID, used in gpu solver to group springs which need to be process in parallel
		/// In cpu solver, this is not needed. 0 means no id is assigned, range 0-255
		/// </summary>
		public int convergenceGroupID = 0;
		
		// you need to SetSimulation in Simualtion class's OnAfterDeserialize() callback. this is a hack
		//becaue unity serializaiton does not keep references.
		public void SetParticles(Simulation sim) {
			this.sim = sim;
			particleA = sim.getParticle(indexA);
			particleB = sim.getParticle(indexB);
			particleM = sim.getParticle(indexM);
		}
		
		//setup by sticks
		//only b rotates around m, if you need a rotate around m, just make another angle constraint
		public AngleConstraint2D(Simulation sim, Spring2D _spring1, Spring2D _spring2) {
			this.on = true;
			this.sim = sim;
			if (_spring1.ParticleA == _spring2.ParticleA) { SetTopology(_spring1.ParticleB,_spring1.ParticleA,_spring2.ParticleB); return; }
			if (_spring1.ParticleA == _spring2.ParticleB) { SetTopology(_spring1.ParticleB,_spring1.ParticleA,_spring2.ParticleA); return; }
			if (_spring1.ParticleB == _spring2.ParticleA) { SetTopology(_spring1.ParticleA,_spring1.ParticleB,_spring2.ParticleB); return; }
			if (_spring1.ParticleB == _spring2.ParticleB) { SetTopology(_spring1.ParticleA,_spring1.ParticleB,_spring2.ParticleA); return; }
		}
		
		void SetTopology(Particle2D pA,Particle2D pM, Particle2D pB) {
			particleA = pA;
			particleB = pB;
			particleM = pM;
			this.indexA = sim.getParticleIndex(pA);
			this.indexB = sim.getParticleIndex(pB);
			this.indexM = sim.getParticleIndex(pM);
			angle_Fixed = GetAngleRadian(pA.Position,pM.Position,pB.Position);
			angle_RestLength2 = Vector2.SqrMagnitude(pA.Position - pB.Position);
		}
		
		public bool ContainSpring(Spring2D spring){
			return ContainParticle(spring.ParticleA) && ContainParticle(spring.ParticleB);
		}
		
		bool ContainParticle(Particle2D p) {
			return particleA == p || particleB == p || particleM == p;
		}
		
		#region toggle
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
		#endregion
		
		public Particle2D ParticleA 
		{
			get {return particleA;}
		}
		
		public Particle2D ParticleB 
		{
			get {return particleB;}
		}
		
		public Particle2D ParticleM
		{
			get {return particleM;}
		}
		
		
		public float angle_RestLength2;
		
		public float delta;
		//this is called by the integrator in each frame
		public void GetDelta() {
			delta = GetDeltaAngle();
		}
		
		public void apply(){
			if (on) {
				
				//float deltaAngle = GetDeltaAngle();
				
				if (delta==0f || Mathf.Abs(delta)<0.01f) return;
				else {
					Vector2 posB = particleB.Position;
					Vector2 posM = particleM.Position;
					if (particleB.IsFree) particleB.Position = Mathp.RotateVector2(posB,posM,-delta * sim.Settings.angleConstant);
					if (particleM.IsFree) particleM.Position = Mathp.RotateVector2(posM,posB,-delta * sim.Settings.angleConstant);
				}
			}
		}
		
		
		public void applyThreaded(){
			if (on) {
				Vector2 posA,posB,posM;
				//lock(particleA) lock(particleB) lock(particleM) {
					posA = particleA.Position;
					posB = particleB.Position;
					posM = particleM.Position;
					angle_Cur = GetAngleRadian(posA,posM,posB);
				//}
				float deltaAngle = GetDeltaAngle();
				if (deltaAngle==0f || Mathf.Abs(deltaAngle)<0.01f) return;
				else {
					
					if (particleA.IsFree) {
						//lock (particleA) {
							Vector2 posA2 = Mathp.RotateVector2(posA,posM,deltaAngle * sim.Settings.angleConstant);
							posA2 = posA2 - posA;
							Extension.InterlockAddFloat(ref particleA.Position.x,posA2.x);
							Extension.InterlockAddFloat(ref particleA.Position.y,posA2.y);
						//}
					}
					if (particleB.IsFree) {
						//lock(particleB) {
							Vector2 posB2 = Mathp.RotateVector2(posB,posM,-deltaAngle * sim.Settings.angleConstant);
							posB2 -= posB;
							Extension.InterlockAddFloat(ref particleB.Position.x,posB2.x);
							Extension.InterlockAddFloat(ref particleB.Position.y,posB2.y);
						//}
						
					}
					if (particleM.IsFree) {
						//lock (particleM) {
							Vector2 posM2 = Mathp.RotateVector2(posM,posA, deltaAngle * sim.Settings.angleConstant);
							posM2 = Mathp.RotateVector2(posM2,posB,-deltaAngle * sim.Settings.angleConstant);
							posM2 -= posM;
							Extension.InterlockAddFloat(ref particleM.Position.x,posM2.x);
							Extension.InterlockAddFloat(ref particleM.Position.y,posM2.y);
						//}
						
					}
				}
				
				
				
			}
		}
		
		float GetAngleRadian(Vector2 g1,Vector2 gm, Vector2 g2) {
			Vector2 s1 = g1 - gm;
			Vector2 s2 = g2 - gm;
			float r = FVector2.SignedAngleRadian(s1,s2);
			if (r<0) r += FVector2.TWOPI;
			return r;
		}
		
		float GetDeltaAngle(){
			angle_Cur = GetAngleRadian(particleA.Position,particleM.Position,particleB.Position);
			return  angle_Cur - angle_Fixed;
		}

		public void DebugDraw(Matrix4x4 local2World,Color angleColor) {
			Vector2 pa = particleM.Position + (particleA.Position - particleM.Position).normalized;
			Vector2 pb = particleM.Position + (particleB.Position - particleM.Position).normalized;
			pa = local2World.MultiplyPoint3x4(pa);
			pb = local2World.MultiplyPoint3x4(pb);
			Debug.DrawLine(pa,pb,angleColor);
		}
		
	}
}

