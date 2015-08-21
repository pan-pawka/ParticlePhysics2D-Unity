using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



namespace ParticlePhysics2D {

	
	public abstract class CollisionObject : MonoBehaviour {
	
		[ReadOnly] public int indexInManager = -1;
		[ReadOnly] public float lastUpdateTime = 0f;

		public Action UpdateMethod {get;set;}//must set the update method in start()
		
		protected static void Connect(CollisionHolder2D obj1, ParticleCollider2D obj2) {
			if (!obj1.connection.Contains(obj2) && !obj2.connection.Contains(obj1)) {
				obj1.connection.Add(obj2);
				obj2.connection.Add(obj1);
				if (obj1.connection.Count==1) PCollision2DManager.Instance.AddCollisionObject(obj1);
				if (obj2.connection.Count==1) PCollision2DManager.Instance.AddCollisionObject(obj2);
				if (PCollision2DManager.IsDebugOn) Debug.Log(obj1.name + " try to connect with " + obj2.name);
			}
		}
		
		protected static void Disconnect(CollisionHolder2D obj1, ParticleCollider2D obj2) {
			if (obj1.connection.Contains(obj2) && obj2.connection.Contains(obj1)) {
				obj1.connection.Remove(obj2);
				obj2.connection.Remove(obj1);
				if (obj1.connection.Count==0) PCollision2DManager.Instance.RemoveCollisionObject(obj1);
				if (obj2.connection.Count==0) PCollision2DManager.Instance.RemoveCollisionObject(obj2);
				if (PCollision2DManager.IsDebugOn) Debug.Log(obj1.name + " try to disconnect with " + obj2.name);
			}
		}
		
		
		
		
	}
}

