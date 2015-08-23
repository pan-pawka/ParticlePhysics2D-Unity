//Yves Wang @ FISH, 2015, All rights reserved
// the collision holder of all the leaf particles
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {

	[RequireComponent(typeof(ParticlePhysics2D.IFormLayer))]
	[ExecuteInEditMode]
	[AddComponentMenu("ParticlePhysics2D/Collision/LeafCollision2D",13)]
	public sealed class LeafCollider2D : CollisionHolder2D {
		
		public int minTraverseDepth = 4;
		public bool isGizmoOn = false;
		public Color gizmoColor = Color.green;
		
		BinaryTree branch;
		List<Particle2D> leafParticles;
		Simulation sim;
		
		void OnResetCollision() {
			this.sim = this.GetComponent<IFormLayer>().GetSimulation;
			leafParticles = sim.getLeafParticles();
			branch = (this.GetComponent<IFormLayer>() as Branch_Mono).GetBinaryTree;
			branch.GetBoundingCircle(sim,base.leafRadius);
		}
		
		void OnClearCollision() {
			this.leafParticles.Clear();
		}
		
		protected override void Start() {
			base.Start();
			OnResetCollision();
			branch = (this.GetComponent<IFormLayer>() as Branch_Mono).GetBinaryTree;
			this.sim = this.GetComponent<IFormLayer>().GetSimulation;
			//branch.GetBoundingCircle(sim,radius);
		}

		void OnDestroy() {
			
		}
		
		void OnEnable() {
			this.GetComponent<IFormLayer>().OnResetForm += OnResetCollision;
			this.GetComponent<IFormLayer>().OnClearForm += OnClearCollision;
		}
		
		void OnDisable() {
			this.GetComponent<IFormLayer>().OnResetForm -= OnResetCollision;
			this.GetComponent<IFormLayer>().OnClearForm -= OnClearCollision;
		}
		
		//the broad phase implementation
		protected override void BroadPhaseUpdate() {
			branch.GetBoundingCircle(sim,base.leafRadius);
		}
		
		private Rigidbody2D targetRb2D;
		private CircleCollider2D cc;
		private Vector2 targetPos;
		private int searchCount = 0;
		public override void TraverseBVHForCircle ( CircleCollider2D cc,out int searchCount ) {
			this.targetRb2D = cc.GetComponent<Rigidbody2D>();
			this.cc = cc;
			this.searchCount = 0;
			if (targetRb2D && this.cc) {
				this.targetPos = transform.InverseTransformPoint(cc.transform.position);
				TraverseBinaryTreeForCircle(this.branch);
			}
			searchCount = this.searchCount;
		}
		
		void TraverseBinaryTreeForCircle ( BinaryTree branch) {
			
			if (branch.branchA!=null && branch.branchB!=null) {
				if (branch.depth < this.minTraverseDepth) {
					TraverseBinaryTreeForCircle(branch.branchA);
					TraverseBinaryTreeForCircle(branch.branchB);	
				} else {
					searchCount ++;
					if (branch.boundingCircle.Overlaps(targetPos,cc.radius)) {
						//branch.boundingCircle.DebugDraw(transform.localToWorldMatrix,branch.depth,Color.white);
						TraverseBinaryTreeForCircle(branch.branchA);
						TraverseBinaryTreeForCircle(branch.branchB);
					}
				}
			} else if (branch.branchA==null && branch.branchB == null){
				Vector2 dir;
				searchCount ++;
				if (branch.boundingCircle.OverlapsResults(targetPos,cc.radius,out dir)){
					branch.boundingCircle.DebugDraw(transform.localToWorldMatrix,branch.depth,Color.magenta);
					//apply collision
					sim.getParticle(branch.leafIndex).Position -= dir;//local space
					Vector2 pos = targetRb2D.transform.position;
					dir = transform.TransformDirection(dir);
					targetRb2D.AddForce(dir * 100f,ForceMode2D.Force);
					
				}
			}
		}
		
		public override void TraverseBVHForPolygon(PolygonCollider2D poly) {}
		
		protected override void OnDrawGizmos() {
			base.OnDrawGizmos();
			if (isGizmoOn && leafParticles!=null) {
				Vector2 pos;
				for (int i=0;i<leafParticles.Count;i++) {
					pos = transform.localToWorldMatrix.MultiplyPoint3x4(leafParticles[i].Position);
					DebugExtension.DrawCircle(pos,Vector3.forward,gizmoColor,base.leafRadius);
				}
			}
		}
		
		
	}
	
}
