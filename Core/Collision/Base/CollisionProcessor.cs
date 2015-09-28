//Yves Wang @ FISH, 2015, All rights reserved

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace ParticlePhysics2D {
	
	//Collision Processor
	//[System.Serializable]
	public sealed class CollisionProcessor {
		
		const int maxProcessNumber = 10;
		const int DEFAULT_CAPACITY = 100;
		
		//[SerializeField]
		private List<CollisionObject> objs = new List<CollisionObject> (DEFAULT_CAPACITY);
		private int _updateHead = -1;
		private int UpdateHead {
			get {
				_updateHead++;
				if (_updateHead >= objs.Count) _updateHead = _updateHead % objs.Count;
				return _updateHead;
			}
		}
		
		public CollisionProcessor() {
		}
		
		public void AddObject(CollisionObject obj) {
			if (obj.indexInManager == -1) {
				objs.Add(obj);
				obj.indexInManager = objs.Count-1;
				obj.lastUpdateTime = Time.realtimeSinceStartup;
				//obj.beginUpdateTime = Time.realtimeSinceStartup;
				if (SimulationManager.Instance.IsDebugOn) Debug.Log( " Object : " + obj.name + " is added to Manager");
			}
		}
		
		public void RemoveObject(CollisionObject obj) {
			if (obj.indexInManager!=-1) {
				CollisionObject temp = objs[obj.indexInManager];
				objs[obj.indexInManager] = objs[objs.Count-1];
				objs[objs.Count-1] = temp;
				objs[obj.indexInManager].indexInManager = objs[objs.Count-1].indexInManager;
				objs[objs.Count-1].indexInManager = -1;
				objs.RemoveAt(objs.Count-1);
				if (SimulationManager.Instance.IsDebugOn) Debug.Log( " Object : " + obj.name + " is removed from Manager");
			}
		}
		
		int updateCount;
		public void Update(float deltaTime) {
			if (objs.Count<=0) return;
			float f = Mathf.Clamp01(deltaTime / SimulationManager.Instance.FixedTimestep_Collision);
			updateCount = Mathf.Clamp((int)(objs.Count * f) , 1 , maxProcessNumber);
			float timeNow = Time.realtimeSinceStartup;
			for (int i=0;i<updateCount;i++) {
				CollisionObject obj = objs[UpdateHead];
				while (timeNow - obj.lastUpdateTime > SimulationManager.Instance.FixedTimestep_Collision) {
					obj.UpdateMethod();
					obj.lastUpdateTime += SimulationManager.Instance.FixedTimestep_Collision;
				}
			}
		}
		
	}
	
	
}
