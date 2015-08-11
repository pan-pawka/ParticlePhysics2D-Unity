//Yves Wang @ FISH, 2015, All rights reserved
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {
	
	[System.Serializable]
	public class MeshLineRender  {
		
		[SerializeField] Simulation sim;
		[SerializeField] Mesh mesh;
		[SerializeField] MeshRenderer meshRenderer;
		[SerializeField] MeshFilter meshFilter;
		public int layer;
		public Color color;
		MaterialPropertyBlock mpb;
		List<Vector3> vert;
		
		static Material mtl;
		Material Mtl {
			get {
				if (mtl==null) mtl = new Material (Shader.Find("ParticlePhysics2D/MeshLineRender"));
				return mtl;
			}
		}
		
		
		public MeshLineRender (Simulation sim,GameObject caller) {
			
			this.sim = sim;
			this.layer = caller.layer;
			this.color = Color.white;
			
			meshRenderer = caller.GetComponent<MeshRenderer>();
			if (meshRenderer == null) meshRenderer = caller.AddComponent<MeshRenderer>();
			meshRenderer.receiveShadows = false;
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			meshRenderer.useLightProbes = false;
			meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			meshRenderer.sharedMaterial = Mtl;
			//meshRenderer.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
			
			mpb = new MaterialPropertyBlock ();
			meshRenderer.GetPropertyBlock(mpb);
			mpb.AddColor("_Color",color);
			meshRenderer.SetPropertyBlock(mpb);
			
			
			mesh = new Mesh ();
			
			//create vertex
			vert = new List<Vector3> (sim.numberOfParticles());
			for (int i=0;i<sim.numberOfParticles();i++) {
				vert.Add(sim.getParticle(i).Position);
			}
			mesh.vertices = vert.ToArray();
			//create edges
			List<int> indices = new List<int> ();
			for (int i=0;i<sim.numberOfSprings();i++){
				int indexA = sim.getParticleIndex(sim.getSpring(i).ParticleA);
				int indexB = sim.getParticleIndex(sim.getSpring(i).ParticleB);
				indices.Add(indexA);
				indices.Add(indexB);
			}
			
			mesh.SetIndices(indices.ToArray(),MeshTopology.Lines,0);
			mesh.RecalculateBounds();
			mesh.MarkDynamic();
		
			meshFilter = caller.GetComponent<MeshFilter>();
			if (meshFilter==null)
				meshFilter = caller.AddComponent<MeshFilter>();
			meshFilter.mesh = mesh;
			//meshFilter.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
			
		}
		
		public void Render(){
			if (mpb==null) {
				mpb = new MaterialPropertyBlock ();
				mpb.AddColor("_Color",color);
			}
			//Graphics.DrawMesh(mesh,Matrix4x4.identity,mtl,layer,cam,0,mpb,false,false);
			
			//get all the vertex world pos
			vert = new List<Vector3> (sim.numberOfParticles());
			for (int i=0;i<sim.numberOfParticles();i++) {
				vert.Add(sim.getParticle(i).Position);
			}
			mesh.vertices = vert.ToArray();
			mesh.RecalculateBounds();
			
			
		}
		
		public void SetColor(Color c) {
			mpb.AddColor("_Color",c);
		}
		
		
		
	}
	
}
