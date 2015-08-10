using UnityEngine;
using System.Collections;
using ParticlePhysics2D;

namespace ParticlePhysics2D {

	[System.Serializable]
	public class AngleConstraint2D : IForce {
		
		[SerializeField] Particle2D particleA, particleM, particleB;
		[SerializeField] float angle_Fixed; //constraint to this angle
		float angle_Cur; // how much the angle is currently
		[SerializeField] float angle_Offset; // how much the angle is currently
		float relaxPercent = 0.1f;//default value
		float k = 1f;
		[SerializeField] bool on;
		
		//setup by sticks
		public AngleConstraint2D(Spring2D _spring1, Spring2D _spring2, float _offset, float _relaxPercent ) {
			angle_Offset = _offset;
			relaxPercent = _relaxPercent;
			on = true;
			if (_spring1.ParticleA == _spring2.ParticleA) { SetTopology(_spring1.ParticleB,_spring1.ParticleA,_spring2.ParticleB); return; }
			if (_spring1.ParticleA == _spring2.ParticleB) { SetTopology(_spring1.ParticleB,_spring1.ParticleA,_spring2.ParticleA); return; }
			if (_spring1.ParticleB == _spring2.ParticleA) { SetTopology(_spring1.ParticleA,_spring1.ParticleB,_spring2.ParticleB); return; }
			if (_spring1.ParticleB == _spring2.ParticleB) { SetTopology(_spring1.ParticleA,_spring1.ParticleB,_spring2.ParticleA); return; }
		}
		
		void SetTopology(Particle2D pA,Particle2D pM, Particle2D pB) {
			particleA = pA;
			particleB = pB;
			particleM = pM;
			angle_Fixed = GetAngle(pA.Position,pM.Position,pB.Position);
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
				angle_Cur = GetAngle(particleA.Position,particleM.Position,particleB.Position);
				float deltaAngle = GetDeltaAngle();
				if (deltaAngle==0f || Mathf.Abs(deltaAngle)<0.05f) return;
				else {
					if (particleA.IsFree) {
						Vector2 curPos1 = Mathp.RotateVector2(particleA.Position,particleM.Position,-deltaAngle*relaxPercent );
						particleA.Force += ( curPos1 - particleA.Position ) * k;
					}
					if (particleB.IsFree) {
						Vector2 curPos1 = Mathp.RotateVector2(particleB.Position,particleM.Position,deltaAngle*relaxPercent );
						particleB.Force += ( curPos1 - particleB.Position ) * k;
					}
					if (particleM.IsFree) {
						Vector2 curPos1 = Mathp.RotateVector2(particleM.Position,particleA.Position,-deltaAngle*relaxPercent );
						curPos1 = Mathp.RotateVector2(curPos1,particleB.Position,deltaAngle*relaxPercent );
						particleM.Force += ( curPos1 - particleM.Position ) * k;
					}
					
				}
			}
		}
		
		float GetAngle(Vector2 g1,Vector2 gm, Vector2 g2) {
			Vector2 s1 = g1 - gm;
			Vector2 s2 = g2 - gm;
			return Quaternion.FromToRotation(s1,s2).eulerAngles.z;
		}
		
		float GetDeltaAngle(){
			float deltaAngle = angle_Cur - angle_Fixed;
			if (deltaAngle>angle_Offset) return deltaAngle-angle_Offset;
			else if (deltaAngle<-angle_Offset) return deltaAngle + angle_Offset;
			else return 0f;
		}
		
	}
}

