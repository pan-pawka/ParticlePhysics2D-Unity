using UnityEngine;
using System.Collections;
using ParticlePhysics2D;
using System;

namespace ParticlePhysics2D {

	[System.Serializable]
	public class AngleConstraint2D : IForce {
		
		[NonSerialized] Particle2D particleA, particleM, particleB;
		[SerializeField] int indexA,indexM,indexB;
		
		//[SerializeField] 
		[SerializeField] public float angle_Fixed; //constraint to this angle
		float angle_Cur; // how much the angle is currently
		[SerializeField] float angle_Offset; // how much the angle is able to be offset

		//[SerializeField] float k;
		[SerializeField] bool on;
		
		[NonSerialized] Simulation sim;
		
		// you need to SetSimulation in Simualtion class's OnAfterDeserialize() callback. this is a hack
		public void SetParticles(Simulation sim) {
			this.sim = sim;
			particleA = sim.getParticle(indexA);
			particleB = sim.getParticle(indexB);
			particleM = sim.getParticle(indexM);
		}
		
		//setup by sticks
		public AngleConstraint2D(Simulation sim, Spring2D _spring1, Spring2D _spring2, float _offset) {
			angle_Offset = _offset;
			on = true;
			this.sim = sim;
			//this.k = 0.1f;
			//this.k = 1f;
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
		
		

		public void apply(){
			if (on) {
				angle_Cur = GetAngleRadian(particleA.Position,particleM.Position,particleB.Position);
				float deltaAngle = GetDeltaAngle();
				if (deltaAngle==0f || Mathf.Abs(deltaAngle)<0.01f) return;
				else {
					Vector2 posA = particleA.Position;
					Vector2 posB = particleB.Position;
					Vector2 posM = particleM.Position;
					if (particleA.IsFree) {
						posA = Mathp.RotateVector2(posA,posM,deltaAngle * sim.angleRelaxPercent);
						particleA.Position = posA;
						//particleA.Force += (posA - particleA.Position) * k;
					}
					if (particleB.IsFree) {
						posB = Mathp.RotateVector2(posB,posM,-deltaAngle * sim.angleRelaxPercent);
						particleB.Position = posB;
						//particleB.Force += (posB - particleB.Position) * k;
					}
					if (particleM.IsFree) {
						posM = Mathp.RotateVector2(posM,posA, deltaAngle * sim.angleRelaxPercent);
						posM = Mathp.RotateVector2(posM,posB,-deltaAngle * sim.angleRelaxPercent);
						particleM.Position = posM;
						//particleM.Force += (posM - particleM.Position) * k;
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
			float deltaAngle = angle_Cur - angle_Fixed;
			if (deltaAngle>angle_Offset) return deltaAngle-angle_Offset;
			else if (deltaAngle<-angle_Offset) return deltaAngle + angle_Offset;
			else return 0f;
		}

		
		static Color angleColor = Color.red;
		public void DebugDraw(Matrix4x4 local2World) {
			Vector2 pa = (particleA.Position + particleM.Position) / 2f;
			Vector2 pb = (particleB.Position + particleM.Position) / 2f;
			pa = local2World.MultiplyPoint3x4(pa);
			pb = local2World.MultiplyPoint3x4(pb);
			Debug.DrawLine(pa,pb,angleColor);
		}
		
	}
}

