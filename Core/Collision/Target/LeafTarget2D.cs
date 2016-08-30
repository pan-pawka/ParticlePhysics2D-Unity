//Yves Wang @ FISH, 2015, All rights reserved
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {

	[RequireComponent(typeof(CircleCollider2D))]
	[AddComponentMenu("ParticlePhysics2D/Collision/LeafCollision2DTarget",13)]
	public sealed class LeafTarget2D : CollisionTarget2D {
		
		private CircleCollider2D circleCollider;
		
		protected override void Start() {
			base.Start();
			if (!circleCollider) circleCollider = this.GetComponent<CircleCollider2D>();
			circleCollider.isTrigger = true;
			circleCollider.offset = Vector2.zero;
		}
		
		private int totalSearchCount = 0;
		private int totalIter = 0;
		protected override void NarrowPhaseUpdate() {
			int searchCount = 0;
			for (int i=0;i<this.connection.Count;i++) {
				int _searchCount;
				connection[i].TraverseBVHForCircle(circleCollider,out _searchCount);
				searchCount += _searchCount;
			}
			totalSearchCount += searchCount;
			totalIter++;
			//Debug.Log(totalSearchCount / totalIter);
		}
		
#if UNITY_EDITOR
		private void OnDrawGizmos() {
			if (!circleCollider) circleCollider = this.GetComponent<CircleCollider2D>();
			Vector2 center = transform.localToWorldMatrix.MultiplyPoint3x4(circleCollider.offset);
			DebugExtension.DrawCircle(center,Vector3.forward,Color.green * 0.8f,circleCollider.radius);
		}
#endif
	}
	
}
