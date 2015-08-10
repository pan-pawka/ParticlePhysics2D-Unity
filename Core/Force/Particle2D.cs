using UnityEngine;
using System.Collections;

namespace ParticlePhysics2D {

	[System.Serializable]
	public class Particle2D  {

		[SerializeField]
		Vector2 position;
		
		[SerializeField]
		Vector2 positionOld;
		
		Vector2 velocity;//used by other integrators other than GPU one
		Vector2 force;
		float mass = 1f;
		
		[SerializeField]
		bool isFixed = false;
	
		public Particle2D()
		{
			position = Vector2.zero;
			velocity = Vector2.zero;
			force = Vector2.zero;
			isFixed = false;
		}
		
		public float distanceTo( Particle2D p ){
			return (this.position-p.position).magnitude;
		}
		
		public float distanceSquaredTo(Particle2D p) {
			return (this.position-p.position).sqrMagnitude;
		}
		
		public void makeFixed(){
			isFixed = true;
			velocity = Vector2.zero;
		}
		public void makeFree(){isFixed = false;}
		
		public void setMass( float m ) {mass = m;}
		
		void reset()
		{
			position = Vector2.zero;
			velocity = Vector2.zero;
			force = Vector2.zero;
			mass = 1f;
		}
		
		public bool IsFixed { get { return isFixed;}}
		
		public bool IsFree {get{return !isFixed;}}
		
		public Vector2 Position {get {return position;}set{position = value;}}
		
		public Vector2 PositionOld {get{return positionOld;}set{positionOld = value;}}
		
		public Vector2 Velocity{get{return velocity;}set{velocity = value;}}
	
		public float Mass {get{return mass;}}

		public Vector2 Force {get{return force;}set{force=value;}}
		
		
	
	}
		
		
	
}