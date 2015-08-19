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
		
		public float radius = 5f;
		public bool isGizmoOn = false;
		public Color gizmoColor = Color.green;
		
		BinaryTree branch;
		float[] boundingSphereRadius;
		
		List<Particle2D> leafParticles;
		Simulation sim;
		
		void OnResetCollision() {
			this.sim = this.GetComponent<IFormLayer>().GetSimulation;
			leafParticles = sim.getLeafParticles();
		}
		
		void OnClearCollision() {
			this.leafParticles.Clear();
		}
		
		protected override void Start() {
			base.Start();
			this.GetComponent<IFormLayer>().OnResetForm += OnResetCollision;
			this.GetComponent<IFormLayer>().OnClearForm += OnClearCollision;
			
			OnResetCollision();
			branch = (this.GetComponent<IFormLayer>() as Branch_Mono).GetBinaryTree;
			this.sim = this.GetComponent<IFormLayer>().GetSimulation;
			boundingSphereRadius = new float[sim.numberOfParticles()];
		}

		void OnDestroy() {
			this.GetComponent<IFormLayer>().OnResetForm -= OnResetCollision;
			this.GetComponent<IFormLayer>().OnClearForm -= OnClearCollision;
		}
		
		void GetBoundingSphereRadius(BinaryTree branch) {
			
		}
		
		//the implementation
		protected override void BroadPhaseUpdate() {
			
		}
		
		// Update is called once per frame
		protected override void Update () {
			base.Update();
		}
		
		
		
		protected override void OnDrawGizmos() {
			base.OnDrawGizmos();
			if (isGizmoOn && leafParticles!=null) {
				Vector2 pos;
				for (int i=0;i<leafParticles.Count;i++) {
					pos = transform.localToWorldMatrix.MultiplyPoint3x4(leafParticles[i].Position);
					DebugExtension.DrawCircle(pos,Vector3.forward,gizmoColor,radius);
				}
			}
		}
		
		
	}
	
}
