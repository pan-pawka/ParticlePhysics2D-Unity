using UnityEngine;
using System.Collections;
using System;

namespace ParticlePhysics2D {

	public class ASConstraint2D : IForce {
		
		[NonSerialized] Particle2D particleA, particleM, particleB;
		[SerializeField] int indexA,indexM,indexB;
		
		//[SerializeField] 
		[SerializeField] float angleLength; //the dist that defining the angle between particleA and particleB
		float angleLengthCur; // how much the angle is currently
		
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
		public ASConstraint2D(Simulation sim, Spring2D _spring1, Spring2D _spring2) {
			on = true;
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
			angleLength = GetAngleLength();
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
				angleLengthCur = GetAngleLength();
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
		
		public float GetAngleLength (){
			return 0f;
		}
		
		public float GetDeltaAngle() {
			return 0f;
		}
		
	
	}

}

