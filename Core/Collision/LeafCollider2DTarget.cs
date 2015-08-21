using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {

	[RequireComponent(typeof(CircleCollider2D))]
	public sealed class LeafCollider2DTarget : ParticleCollider2D {
		
		private CircleCollider2D circleCollider;
		
		protected override void Start() {
			base.Start();
			if (!circleCollider) circleCollider = this.GetComponent<CircleCollider2D>();
			circleCollider.isTrigger = true;
			circleCollider.offset = Vector2.zero;
		}
		
		protected override void NarrowPhaseUpdate() {
			for (int i=0;i<this.connection.Count;i++) {
				connection[i].TraverseBVHForCircle(circleCollider);
			}
		}
		
		private void OnDrawGizmos() {
			if (!circleCollider) circleCollider = this.GetComponent<CircleCollider2D>();
			Vector2 center = transform.localToWorldMatrix.MultiplyPoint3x4(circleCollider.offset);
			DebugExtension.DrawCircle(center,Vector3.forward,Color.green * 0.8f,circleCollider.radius);
		}
	}
	
}
