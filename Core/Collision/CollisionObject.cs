using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



namespace ParticlePhysics2D {

	public enum PhaseType {Broad, Narrow}

	public abstract class CollisionObject : MonoBehaviour {
	
		public int indexInManager = -1;
		public float lastUpdateTime = 0f;
//		public float beginUpdateTime = 0f;
//		public int updateCount = 0;
		public Action UpdateMethod {get;set;}//must set the update method in start()
		
		public PhaseType phaseType;//derived class must specify the phase type
		
		const int DEFAULT_CONNECTION_NUM = 10;
		
		[HideInInspector]
		public List<CollisionObject> connection = new List<CollisionObject> (DEFAULT_CONNECTION_NUM);
		
		private CircleCollider2D circle;
		
		//derived class must extend Start to set the update method
		protected virtual void Start(){
			circle = this.GetComponent<CircleCollider2D>();
			circle.isTrigger = true;
		}
		
		//conenction between narrow phase obj and broad phase obj
		protected void Connect(CollisionObject obj) {
			if (this.phaseType != obj.phaseType && !connection.Contains(obj) && !obj.connection.Contains(this)) {
				connection.Add(obj);
				obj.connection.Add(this);
				if (obj.connection.Count==1) PCollision2DManager.Instance.AddCollisionObject(obj);
				if (this.connection.Count==1) PCollision2DManager.Instance.AddCollisionObject(this);
				if (PCollision2DManager.IsDebugOn) Debug.Log(this.name + " try to connect with " + obj.name);
			}
		}
		
		protected void Disconnect (CollisionObject obj) {
			if (this.phaseType != obj.phaseType && connection.Contains(obj) && obj.connection.Contains(this)) {
				connection.Remove(obj);
				obj.connection.Remove(this);
				if (connection.Count==0) PCollision2DManager.Instance.RemoveCollisionObject(this);
				if (obj.connection.Count==0) PCollision2DManager.Instance.RemoveCollisionObject(obj);
				if (PCollision2DManager.IsDebugOn) Debug.Log(this.name + " try to disconnect with " + obj.name);
			}
		}
		
		protected virtual void OnDrawGizmos() {
			if (!circle) circle = this.GetComponent<CircleCollider2D>();
			Vector2 center = transform.localToWorldMatrix.MultiplyPoint3x4(circle.offset);
			DebugExtension.DrawCircle(center,Vector3.forward,Color.green,circle.radius);
		}
		
		
	}
}

