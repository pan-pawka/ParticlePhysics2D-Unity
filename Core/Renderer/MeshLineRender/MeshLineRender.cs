//Yves Wang @ FISH, 2015, All rights reserved
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ParticlePhysics2D {
	
	
	[RequireComponent(typeof(ParticlePhysics2D.IFormLayer),typeof(MeshRenderer),typeof(MeshFilter))]
	[ExecuteInEditMode]
	[AddComponentMenu("ParticlePhysics2D/Renderer/MeshLineRender",13)]
	public class MeshLineRender : MonoBehaviour {
		
		#region Fields and Properties
		Simulation sim;
		[HideInInspector] [SerializeField] Mesh mesh;
		[HideInInspector] [SerializeField] MeshRenderer meshRenderer;
		[HideInInspector] [SerializeField] MeshFilter meshFilter;
		[HideInInspector] [SerializeField] int particleNumCache;
		[HideInInspector] [SerializeField] int stringNumCache;
		[HideInInspector] [SerializeField] Color colorCache = Color.white;
		
		public Color color = Color.white;
		
		MaterialPropertyBlock mpb;
		
		static Material mtl;
		static Material Mtl {
			get {
				if (mtl==null) mtl = new Material (Shader.Find("ParticlePhysics2D/MeshLineRender"));
				return mtl;
			}
		}
		
		//if the data has been initialized already?called by editor
		public bool IsInitialized {
			get {
				if ( mesh!=null && meshRenderer!=null && meshFilter!=null) {
					//Debug.LogWarning("The Mesh Line Renderer's resources are initialized!");
					return true;
				} else {
					Debug.Log("The Mesh Line Renderer's resources have not been initialized!");
					return false;
				}
			}
		}
		#endregion
		
		#region Unity Methods
		
		void Start () {
			OnResetRender();
		}
		
		void OnEnable() {
			this.GetComponent<IFormLayer>().OnResetForm += OnResetRender;
		}
		
		void OnDisable() {
			this.GetComponent<IFormLayer>().OnResetForm -= OnResetRender;
		}
		
		public void LateUpdate(){
//			if (mpb==null) {
//				mpb = new MaterialPropertyBlock ();
//				mpb.AddColor("_Color",color);
//			}
			
			if (mesh!=null) {
				//if the sim changes data topology
				if (this.particleNumCache!=sim.numberOfParticles() || this.stringNumCache != sim.numberOfSprings()) {
					//Debug.Log(this.particleNumCache);
					//Debug.Log(sim.numberOfParticles());
					mesh.Clear();
					CreateMesh();
					this.particleNumCache=sim.numberOfParticles();
					this.stringNumCache = sim.numberOfSprings();
				} else {
					Vector3[] v = sim.getVertices();
					if (v!=null) {
						mesh.vertices = v;
						mesh.RecalculateBounds();
					}
					//
					else {
					
					}
				}
				
			}
			
			if (color != colorCache) {
				SetColor(color);
				colorCache = color;
			}
		}
		
		#endregion
		
		void OnResetRender() {
			this.sim = this.GetComponent<IFormLayer>().GetSimulation;
			mpb = new MaterialPropertyBlock ();
			SetColor(color);
		}
		
		//called by editor script to set up data
		public void MeshLineRender_Ctor () {
			
			this.sim = this.GetComponent<IFormLayer>().GetSimulation;
			this.particleNumCache = sim.numberOfParticles();
			this.stringNumCache = sim.numberOfSprings();
			
			meshRenderer = this.GetComponent<MeshRenderer>();
			if (meshRenderer == null) meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
			meshRenderer.receiveShadows = false;
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			meshRenderer.useLightProbes = false;
			meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			meshRenderer.sharedMaterial = Mtl;
			meshRenderer.hideFlags =  HideFlags.HideInInspector;
			
			mpb = new MaterialPropertyBlock ();
			meshRenderer.GetPropertyBlock(mpb);
			mpb.AddColor("_Color",color);
			meshRenderer.SetPropertyBlock(mpb);
			colorCache = color;
			
			if (mesh==null) {
				mesh = new Mesh ();
				CreateMesh();
				this.particleNumCache = sim.numberOfParticles();
				this.stringNumCache = sim.numberOfSprings();
			}
			
			meshFilter = this.GetComponent<MeshFilter>();
			if (meshFilter==null)
				meshFilter = this.gameObject.AddComponent<MeshFilter>();
			meshFilter.mesh = mesh;
			meshFilter.hideFlags =  HideFlags.HideInInspector;
			
		}
		
		
		
		public void SetColor(Color c) {
			if (mpb==null) mpb = new MaterialPropertyBlock ();
			if (meshRenderer) {
				meshRenderer.GetPropertyBlock(mpb);
				mpb.AddColor("_Color",c);
				meshRenderer.SetPropertyBlock(mpb);
			}
		}
		
		void CreateMesh () {
			
			if (sim.numberOfParticles()<2) {
				mesh.Clear();
				return;
			}
			
			//create vertex
			Vector3[] v = sim.getVertices();
			if (v==null) {
				mesh.Clear();
				return;
			}
			else {
				mesh.vertices = v;
			}
			
			//create edges
			int[] ic = sim.getIndices();
			if (ic==null) {
				mesh.Clear();
				return;
			} else {
				mesh.SetIndices(ic,MeshTopology.Lines,0);
				mesh.RecalculateBounds();
				mesh.MarkDynamic();
				
			}
			
		}
		
		public void RemoveResources() {

			ObjExtension.ObjDestroy(meshRenderer);
			ObjExtension.ObjDestroy(meshFilter);
			ObjExtension.ObjDestroy(mesh);
			
		}
		
		
		
	}
	
	
	
}
