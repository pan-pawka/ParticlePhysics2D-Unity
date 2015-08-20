using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



namespace ParticlePhysics2D {

	public enum PhaseType {Broad, Narrow}

	public abstract class CollisionObject : MonoBehaviour {
	
		public int indexInManager = -1;
		public float lastUpdateTime = 0f;
		public Action UpdateMethod {get;set;}//must set the update method in start()
		
		public PhaseType phaseType;//derived class must specify the phase type
		
		const int DEFAULT_CONNECTION_NUM = 10;
		
		public List<CollisionObject> connection = new List<CollisionObject> (DEFAULT_CONNECTION_NUM);
		
		//derived class must override Start to set the update method
		protected abstract void Start();
		
		//conenction between narrow phase obj and broad phase obj
		protected void Connect(CollisionObject obj) {
			if (this.phaseType != obj.phaseType && !connection.Contains(obj) && !obj.connection.Contains(this)) {
				connection.Add(obj);
				obj.connection.Add(this);
				if (obj.connection.Count==1) PCollision2DManager.AddCollisionObject(obj);
				if (this.connection.Count==1) PCollision2DManager.AddCollisionObject(this);
				if (PCollision2DManager.IsDebugOn) Debug.Log(this.name + " try to connect with " + obj.name);
			}
		}
		
		protected void Disconnect (CollisionObject obj) {
			if (this.phaseType != obj.phaseType && connection.Contains(obj) && obj.connection.Contains(this)) {
				connection.Remove(obj);
				obj.connection.Remove(this);
				if (connection.Count==0) PCollision2DManager.RemoveCollisionObject(this);
				if (obj.connection.Count==0) PCollision2DManager.RemoveCollisionObject(obj);
				if (PCollision2DManager.IsDebugOn) Debug.Log(this.name + " try to disconnect with " + obj.name);
			}
		}
		
		
	}
}

