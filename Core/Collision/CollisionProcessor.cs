//Yves Wang @ FISH, 2015, All rights reserved

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace ParticlePhysics2D {
	
	//Collision Processor
	public sealed class CollisionProcessor {
		
		const float fixedTimestep = 1f/30f; //fps = 30
		const int maxProcessNumber = 10;
		const int DEFAULT_CAPACITY = 100;
		bool isDebugOn = true;
		
		private List<CollisionObject> objs = new List<CollisionObject> (DEFAULT_CAPACITY);
		private int _updateHead = -1;
		private int UpdateHead {
			get {
				_updateHead++;
				if (_updateHead >= objs.Count) _updateHead = _updateHead % objs.Count;
				return _updateHead;
			}
		}
		
		public CollisionProcessor(bool debug) {
			this.isDebugOn = debug;
		}
		
		public void AddObject(CollisionObject obj) {
			if (obj.indexInManager == -1) {
				objs.Add(obj);
				obj.indexInManager = objs.Count-1;
				obj.lastUpdateTime = Time.realtimeSinceStartup;
				if (isDebugOn) Debug.Log(obj.phaseType + " Object : " + obj.name + " is added to Manager");
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
				if (isDebugOn) Debug.Log(obj.phaseType + " Object : " + obj.name + " is removed from Manager");
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
