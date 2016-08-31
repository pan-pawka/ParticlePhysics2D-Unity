//Yves Wang @ FISH, 2015, All rights reserved
using UnityEngine;
using System.Collections;
using System;

namespace ParticlePhysics2D {

	[System.Serializable]
	public class Particle2D   {

		[SerializeField]
		public Vector2 Position;
		
		[System.NonSerialized]
		public Vector2 PositionOld;

		float mass = 1f;
		
		
		[SerializeField]
		bool isFixed = false;
		
		[SerializeField]
		bool isLeaf = false;
		public bool IsLeaf {
			get {return isLeaf;}
			set {isLeaf = value;}
		}
		
	
		public Particle2D()
		{
			this.Position = Vector2.zero;
			this.isFixed = false;
			//this.force = Vector2.zero;
		}
		
		public float distanceTo( Particle2D p ){
			return (this.Position-p.Position).magnitude;
		}
		
		public float distanceSquaredTo(Particle2D p) {
			return (this.Position-p.Position).sqrMagnitude;
		}
		
		public void makeFixed(){
			isFixed = true;
		}
		public void makeFree(){isFixed = false;}
		
		public void setMass( float m ) {mass = m;}
		
		void reset()
		{
			Position = Vector2.zero;
			mass = 1f;
		}
		
		public bool IsFixed { get { return isFixed;}}
		
		public bool IsFree {get{return !isFixed;}}
	
		public float Mass {get{return mass;}}

	
	}
		
		
	
}