using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {

	[RequireComponent(typeof(ParticlePhysics2D.IFormLayer))]
	[ExecuteInEditMode]
	[AddComponentMenu("ParticlePhysics2D/Collision/LeafCollision2D",13)]
	public class LeafCollision2D : MonoBehaviour {
		
		public float radius = 5f;
		public bool isGizmoOn = false;
		public Color gizmoColor = Color.green;
		
		List<Particle2D> leafParticles;
		Simulation sim;
		
		void OnResetCollision() {
			this.sim = this.GetComponent<IFormLayer>().GetSimulation;
			leafParticles = sim.getLeafParticles();
		}
		
		void OnClearCollision() {
			this.leafParticles.Clear();
		}
		
		
		
		void Start() {
			this.GetComponent<IFormLayer>().OnResetForm += OnResetCollision;
			this.GetComponent<IFormLayer>().OnClearForm += OnClearCollision;
			OnResetCollision();
		}

		void OnDestroy() {
			this.GetComponent<IFormLayer>().OnResetForm -= OnResetCollision;
			this.GetComponent<IFormLayer>().OnClearForm -= OnClearCollision;
		}
		
		// Update is called once per frame
		void Update () {
			
		}
		
		void OnDrawGizmos() {
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
