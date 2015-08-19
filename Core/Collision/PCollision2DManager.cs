//Yves Wang @ FISH, 2015, All rights reserved

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {

	[AddComponentMenu("ParticlePhysics2D/Collision/Manager",13)]
	public class PCollision2DManager : Singleton<PCollision2DManager> {
		
		//singleton instance
		public static PCollision2DManager Instance {
			get {
				return ((PCollision2DManager)mInstance);
			} set {
				mInstance = value;
			}
		}
		
		//static ctor
		static PCollision2DManager () {
			Debug.Log("Collision Manager created : " + PCollision2DManager.Instance.gameObject.name);
		}
		
		const float fixedTimestep = 1f/30f; //fps = 30
		const int maxProcessNumber = 10;
		const int DEFAULT_CAPACITY = 100;
		
		#region Broad phase
		//list of collision holders
		static List<CollisionHolder2D> collisionHolders = new List<CollisionHolder2D> (DEFAULT_CAPACITY);
		
		public static void AddCollisionHolder (CollisionHolder2D c) {
			if (!collisionHolders.Contains(c)) {
				collisionHolders.Add(c);
				c.lastUpdateTime = Time.realtimeSinceStartup;
			}
		}
		
		public static void RemoveCollisionHolder (CollisionHolder2D c) {
			if (collisionHolders.Contains(c)) collisionHolders.Remove(c);
		}
		
		static int _updateHead1 = -1;
		static int UpdateHead1 {
			get {
				_updateHead1++;
				if (_updateHead1 >= collisionHolders.Count) _updateHead1 = _updateHead1 % collisionHolders.Count;
				return _updateHead1;
			}
		}
		#endregion
		
		#region narrow Phase
		//particle collider 2d list
		static List<ParticleCollider2D> pCollider2D = new List<ParticleCollider2D> (DEFAULT_CAPACITY);
		//return the index of the added 
		public static void AddPCollider2D( ParticleCollider2D pc) {
			if (pc.indexInManager == -1) {
				pc.lastUpdateTime = Time.realtimeSinceStartup;
				pCollider2D.Add(pc);
				pc.indexInManager = pCollider2D.Count-1;
			}
		}
		public static void RemovePCollider2D ( ParticleCollider2D pc) {
			//if (pCollider2D.Contains(pc)) pCollider2D.Remove(pc);
			ParticleCollider2D lastOne = pCollider2D[pCollider2D.Count-1];
			lastOne.indexInManager = pc.indexInManager;
			Swap(pCollider2D[pc.indexInManager],lastOne);
			pCollider2D[pc.indexInManager].indexInManager = -1;
			pCollider2D.RemoveAt(pCollider2D.Count-1);
		}
		
		static int _updateHead2 = -1;
		static int UpdateHead2 {
			get {
				_updateHead2++;
				if (_updateHead2 >= pCollider2D.Count) _updateHead2 = _updateHead2 % pCollider2D.Count;
				return _updateHead2;
			}
		}
		
		static void Swap ( ParticleCollider2D a, ParticleCollider2D b) {
			ParticleCollider2D temp = a;
			a = b;
			b = temp;
		}
		
		#endregion
		
		int framesCount;
		int broadPhaseCount;
		int narrowPhaseCount;
		
		void FixedUpdate () {
			
			//Np = Ntotal * Time.deltaTime / fixedTimeStep;
			float f = Mathf.Clamp01(Time.deltaTime / fixedTimestep);
			//how many Broad Phase BVH update should we process in each update, a collision holder will have one BVH calculation
			broadPhaseCount = Mathf.Min(maxProcessNumber,(int)(collisionHolders.Count * f));
			//how many narrow phase update should we perform in each update
			narrowPhaseCount = Mathf.Min(maxProcessNumber,(int)(pCollider2D.Count * f));
			
			float timeNow = Time.realtimeSinceStartup;
			
			//broad phase update
			for (int i=0;i<broadPhaseCount;i++) {
				CollisionHolder2D p2d = collisionHolders[UpdateHead1];
				while (timeNow - p2d.lastUpdateTime > fixedTimestep) {
					//p2d.();
					p2d.lastUpdateTime += fixedTimestep;
				}
			}
			
			//narrow phase update
			for (int i=0;i<narrowPhaseCount;i++) {
				ParticleCollider2D p2d = pCollider2D[UpdateHead2];
				while (timeNow - p2d.lastUpdateTime > fixedTimestep ) {
					//p2d.UpdateMethod();
					p2d.lastUpdateTime += fixedTimestep;
				}
			}
		}
	}
	
	//Collision Processor
	public sealed class CollisionProcessor {
		
		const float fixedTimestep = 1f/30f; //fps = 30
		const int maxProcessNumber = 10;
		const int DEFAULT_CAPACITY = 100;
		private List<CollisionObject> objs = new List<CollisionObject> (DEFAULT_CAPACITY);
		private int _updateHead = -1;
		private int UpdateHead {
			get {
				_updateHead++;
				if (_updateHead >= objs.Count) _updateHead = _updateHead % objs.Count;
				return _updateHead;
			}
		}
		
		public void AddObject(CollisionObject obj) {
			if (obj.indexInManager == -1) {
				objs.Add(obj);
				obj.indexInManager = objs.Count-1;
				obj.lastUpdateTime = Time.realtimeSinceStartup;
			}
		}
		
		public void RemoveObject(CollisionObject obj) {
			if (obj.indexInManager!=-1) {
				CollisionObject thisOne = objs[obj.indexInManager];
				CollisionObject lastOne = objs[objs.Count-1];
				lastOne.indexInManager = obj.indexInManager;
				Swap(lastOne,thisOne);
				thisOne.indexInManager = -1;
				objs.RemoveAt(objs.Count-1);
			}
		}
		
		int updateCount;
		public void Update(float deltaTime) {
			float f = Mathf.Clamp01(deltaTime / fixedTimestep);
			updateCount = Mathf.Min(maxProcessNumber,(int)(objs.Count * f)); 
			float timeNow = Time.realtimeSinceStartup;
			
			for (int i=0;i<updateCount;i++) {
				CollisionObject obj = objs[UpdateHead];
				while (timeNow - obj.lastUpdateTime > fixedTimestep) {
					obj.UpdateMethod();
					obj.lastUpdateTime += fixedTimestep;
				}
			}
		}
		
		void Swap ( CollisionObject t1,CollisionObject t2) {
			CollisionObject t = t1;
			t1 = t2;
			t2 = t;
		}
	}

}

