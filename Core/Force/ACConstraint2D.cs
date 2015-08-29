//
//using UnityEngine;
//using System.Collections;
//using System;
//
//namespace ParticlePhysics2D {
//
//	/// <summary>
//	/// Angle-Consine Constraint
//	/// </summary>
//	[Serializable]
//	public class ACConstraint2D : IForce {
//		
//		[NonSerialized] Particle2D particleA, particleM, particleB;
//		[SerializeField] int indexA,indexM,indexB;
//		
//		const float angleConstant = 0.001f;
//		[SerializeField] float angleCos; //the cosine of the initial angle
//		[SerializeField] float lengthMultiplar;//the length of spring a and spring b;
//		float angleCosCur; // how much the angle cosine is currently
//		
//		[SerializeField] bool on;
//		
//		[NonSerialized] Simulation sim;
//		
//		// you need to SetSimulation in Simualtion class's OnAfterDeserialize() callback. this is a hack
//		public void SetParticles(Simulation sim) {
//			this.sim = sim;
//			particleA = sim.getParticle(indexA);
//			particleB = sim.getParticle(indexB);
//			particleM = sim.getParticle(indexM);
//		}
//		
//		
//		//setup by sticks
//		public ACConstraint2D(Simulation sim, Spring2D _spring1, Spring2D _spring2) {
//			on = true;
//			this.sim = sim;
//			if (_spring1.ParticleA == _spring2.ParticleA) { SetTopology(_spring1.ParticleB,_spring1.ParticleA,_spring2.ParticleB); return; }
//			if (_spring1.ParticleA == _spring2.ParticleB) { SetTopology(_spring1.ParticleB,_spring1.ParticleA,_spring2.ParticleA); return; }
//			if (_spring1.ParticleB == _spring2.ParticleA) { SetTopology(_spring1.ParticleA,_spring1.ParticleB,_spring2.ParticleB); return; }
//			if (_spring1.ParticleB == _spring2.ParticleB) { SetTopology(_spring1.ParticleA,_spring1.ParticleB,_spring2.ParticleA); return; }
//		}
//		
//		void SetTopology(Particle2D pA,Particle2D pM, Particle2D pB) {
//			particleA = pA;
//			particleB = pB;
//			particleM = pM;
//			this.indexA = sim.getParticleIndex(pA);
//			this.indexB = sim.getParticleIndex(pB);
//			this.indexM = sim.getParticleIndex(pM);
//			float lengthA = (pA.Position - pM.Position).magnitude;
//			float lengthB = (pB.Position - pM.Position).magnitude;
//			lengthMultiplar = lengthA * lengthB;
//			//get the initial angle cos
//			Vector2 a = particleA.Position - particleM.Position;
//			Vector2 b = particleB.Position - particleM.Position;
//			float dotP = FVector2.Dot(a,b);
//			float crossP = FVector2.Cross(a,b);
//			angleCos = (dotP/lengthMultiplar + 1f) * Math.Sign(crossP);
//		}
//		
//		public bool ContainSpring(Spring2D spring){
//			return ContainParticle(spring.ParticleA) && ContainParticle(spring.ParticleB);
//		}
//		
//		bool ContainParticle(Particle2D p) {
//			return particleA == p || particleB == p || particleM == p;
//		}
//		
//		#region toggle
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
//		public bool isOn()
//		{
//			return on;
//		}
//		
//		public bool isOff()
//		{
//			return !on;
//		}
//		#endregion
//		
//		public Particle2D ParticleA 
//		{
//			get {return particleA;}
//		}
//		
//		public Particle2D ParticleB 
//		{
//			get {return particleB;}
//		}
//		
//		public Particle2D ParticleM
//		{
//			get {return particleM;}
//		}
//		
//		private Vector2 da,db;
//		public void apply(){
//			if (on) {
//				
//				if (!GetDeltaVector(out da,out db)) return;
//				else {
//					if (particleA.IsFree) {
//						//Debug.Log(da);
//						particleA.Position += da * angleConstant;
//					}
//					if (particleB.IsFree) {
//						particleB.Position += db * angleConstant;
//					}
////					if (particleM.IsFree) {
////						
////					}
//				}
//			}
//		}
//		
////		public void apply()
////		{	
////			if ( on && ( a.IsFree || b.IsFree ) )
////			{
////				//faster square root approx from Advanced Character Physics
////				Vector2 delta = a.Position - b.Position;
////				delta *= restLength2 /(delta.sqrMagnitude + restLength2) - 0.5f;
////				if (a.IsFree) a.Position += delta * sim.springConstant;
////				if (b.IsFree) b.Position -= delta * sim.springConstant;
////			}
////		}
//		
//		public bool GetDeltaVector (out Vector2 deltaA,out Vector2 deltaB){
//			Vector2 a = particleA.Position - particleM.Position;
//			Vector2 b = particleB.Position - particleM.Position;
//			float dotP = FVector2.Dot(a,b);
//			float crossP = FVector2.Cross(a,b);
//			angleCosCur = (dotP/lengthMultiplar + 1f) * Math.Sign(crossP);
//			float deltaCos = (angleCosCur - angleCos)/2f;
//			float deltaCasAbs = Mathf.Abs(deltaCos);
//			if (deltaCasAbs < 0.01) {
//				deltaA = Vector2.zero;
//				deltaB = Vector2.zero;
//				return false;
//			}
//			else {
//				deltaA = FVector2.CrossUnitZ(a,deltaCos) * deltaCasAbs / 2f;
//				deltaB = FVector2.CrossUnitZ(b,-deltaCos) * deltaCasAbs / 2f;
//				return true;
//			}
//		}
//		
//		
//		public void DebugDraw(Matrix4x4 local2World) {
//			Vector2 pa = (particleA.Position + particleM.Position) / 2f;
//			Vector2 pb = (particleB.Position + particleM.Position) / 2f;
//			//Debug.Log(pa + " = " + pb);
//			pa = local2World.MultiplyPoint3x4(pa);
//			pb = local2World.MultiplyPoint3x4(pb);
//			Debug.DrawLine(pa,pb,Color.red);
//		}
//		
//	
//	}
//
//}
//
