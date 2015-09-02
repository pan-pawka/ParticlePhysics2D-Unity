//Yves Wang @ FISH, 2015, All rights reserved
/// <summary>
/// Sim buffer.
/// The buffer hold all the data for the gpu integrator
/// </summary>

using UnityEngine;
using System.Collections;

namespace ParticlePhysics2D {

	public class SimBuffer {
		
		RenderTexture[] poRT = new RenderTexture[2];// old position
		RenderTexture StateRT {get;set;}
		Simulation sim;
		int ID_PositionCache,ID_Damping;
		float damping,springConstant,angleRelaxPercent;
		Mesh springMesh,angleMesh;
		int current = 0,next = 1;//for position tex
		int width,height;// the rendertexture size for the particle data structure
		
		public RenderTexture PositionRT {get;set;}
		private Texture2D tempPos;
		private Rect tempRect;
		
		#region Ctor
		
		public static SimBuffer Create(Simulation sim) {
			int x,y;
			float u;
			if (SimBuffer.GetTexDimension(sim.numberOfParticles(),out x,out y,out u)) {
				return new SimBuffer (x,y,sim);
			} else {
				Debug.LogError("Cannot create GPU rendertexture with wrong dimension");
				return null;
			}
		}
		
		//create the simulation gpu buffer
		private SimBuffer(int x,int y,Simulation sim) {
			this.width = x;
			this.height = y;
			this.sim = sim;
			this.ID_PositionCache = Shader.PropertyToID("_PositionCache");
			this.ID_Damping = Shader.PropertyToID("_Damping");
			for (int i=0;i<2;i++) {
				poRT[i] = new RenderTexture (x,y,0,RenderTextureFormat.RGFloat);
				poRT[i].useMipMap = false;
				poRT[i].anisoLevel = 1;
				poRT[i].filterMode = FilterMode.Point;
			}
			Init();
		}
		
		/// <summary>
		/// If we need this GPU solver, we should re-allocate in init;
		/// </summary>
		public void Init() {
			
			SendToGPU_ParticleState();
			
			//initialize the temporary texture2d for exchanging position bewteen cpu and gpu
			if (tempPos==null) {
				tempPos = new Texture2D (width,height,TextureFormat.RGFloat,false);
				tempPos.filterMode = FilterMode.Point;
				tempPos.anisoLevel = 0;
			}
			
			//initialize the rect for readpixels
			tempRect = new Rect (0f,0f,(float)width,(float)height);
			
			SendToGPU_ParticlePosition();
			
			for (int i=0;i<2;i++) {
				if (!poRT[i].IsCreated()) poRT[i].Create();
				Graphics.Blit(PositionRT,poRT[i]);
			}
		}
		
		
		#endregion
		
		#region Spring Constraint
		public void SpringConstraint() {
			if (springMesh==null) {
				springMesh = new Mesh ();
				Vector3[] vert = new Vector3[sim.numberOfSprings()];
				springMesh.vertices = vert;
			} 
		}
		#endregion
		
		#region Collision-Constraint-Verlet
		/// <summary>
		/// This is mainly called in the collsion gpu solver, where the positon of particles are read and wirte multiple times
		/// </summary>
		public void BlitPosition (Material mtl,int pass = -1) {
			RenderTexture temp = RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.RGFloat);
			Graphics.Blit(PositionRT,temp,mtl,pass);
			RenderTexture.ReleaseTemporary(PositionRT);
			PositionRT = temp;
		}
		
		/// <summary>
		/// This is called by constraint solver, to blit the final results in the cache, 
		/// for the later use in the verlet of the next frame.If positon tex need to be processed
		/// in multiple n pass, use BlitPosition in all the n-1 pass, and BlitPositionToCache in the last pass
		/// </summary>
		public void BlitPositionToCache(Material mtl,int pass = -1) {
			Graphics.Blit(PositionRT,poRT[next],mtl,pass);
			RenderTexture.ReleaseTemporary(PositionRT);
		}
		
		/// <summary>
		/// Verlet the result of this frame and the prev frame, to PositionRT
		/// </summary>
		public void Verlet (Material mtl,int pass = -1) {
			mtl.SetTexture(ID_PositionCache,poRT[current]);
			if (damping != sim.damping) {
				damping = sim.damping;
				mtl.SetFloat(ID_Damping,damping);
			}
			PositionRT = RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.RGFloat);
			Graphics.Blit(poRT[next],PositionRT,mtl,pass);
			poRT[current].DiscardContents();
			current ++; current %= 2;
			next ++; next %= 2;
		}
		#endregion
		
		#region Data transfer between cpu and gpu
		//this will get the particles state into StateRT
		void SendToGPU_ParticleState ( ) {
			//the state of particle, like IsFree...
			RenderTexture.ReleaseTemporary(StateRT);
			StateRT = RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.R8);
			Texture2D state2d = new Texture2D (width,height,TextureFormat.RHalf,false,false);
			state2d.filterMode = FilterMode.Point;
			state2d.anisoLevel = 0;
			Color[] pc = new Color[width * height];
			int counter = 0;
			for (int i=0;i<height;i++) {
				for (int j=0;i<width;j++) {
					if (counter<sim.numberOfParticles()) {
						if (sim.getParticle(counter).IsFree ) {
							pc[counter] = new Color (1f,0f,0f);
						} else {
							pc[counter] = new Color (0f,0f,0f);
						}
					} else {
						pc[counter] = new Color (0f,0f,0f);
					}
					counter++;
				}
			}
			state2d.SetPixels(pc);
			state2d.Apply();
			Graphics.Blit(state2d,StateRT);
			Extension.ObjDestroy(state2d);
		}
		
		/// <summary>
		/// this will get all the particle position into PositionRT, usually after collision response
		/// </summary>
		public void SendToGPU_ParticlePosition() {
			Color[] pc = new Color[width * height];
			int counter = 0;
			for (int i=0;i<height;i++) {
				for (int j=0;i<width;j++) {
					Vector2 pPos;
					if (counter<sim.numberOfParticles()) {
						pPos = sim.getParticle(counter).Position;
					} else {
						pPos = Vector2.zero;
					}
					pc[counter] = new Color (pPos.x,pPos.y,0f);
					counter++;
				}
			}
			
			tempPos.SetPixels(pc);
			tempPos.Apply(false);
			
			if (PositionRT) RenderTexture.ReleaseTemporary(PositionRT);
			PositionRT = RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.RGFloat);
			
			Graphics.Blit(tempPos,PositionRT);
		}
		//this will transfer data in PositionRT into the particle list
		public void SendToCPU_ParticlePosition() {
			RenderTexture rtcache = RenderTexture.active;
			RenderTexture.active = PositionRT;
			tempPos.ReadPixels(tempRect,0,0,false);
			RenderTexture.active = rtcache;
			Color[] pc;
			pc = tempPos.GetPixels();
			int counter = 0;
			for (int i=0;i<height;i++) {
				for (int j=0;i<width;j++) {
					if (counter<sim.numberOfParticles()) {
						Vector2 pos = new Vector2 (pc[counter].r,pc[counter].g);
						sim.getParticle(counter).Position = pos;
					}
					counter++;
				}
			}
			
		}
		#endregion
		
		/// <summary>
		/// If the simualtion goes out of scope, we can release the hardware resources manually
		/// </summary>
		public void Release() {
			if (PositionRT) RenderTexture.ReleaseTemporary(PositionRT);
			for (int i=0;i<2;i++) {
				poRT[i].Release();
			}
			RenderTexture.ReleaseTemporary(StateRT);
			Extension.ObjDestroy(tempPos);
		}
		
		public static bool GetTexDimension(int particles, out int x, out int y,out float usage) {
			if (particles<1) {
				x = y = 0;
				usage = 0f;
				return false;
			} else {
				int po2 = Mathf.CeilToInt(Mathf.Log(particles,2f));
				if (po2 % 2 == 0) {
					x = po2 /2;
					y = x;
				} else {
					x = po2 / 2 + 1;
					y = po2 - x;
				}
				x = (int)Mathf.Pow(2f,x);
				y = (int)Mathf.Pow(2f,y);
				usage = (float)particles / (float)( x * y );
				return true;
			}
			
		}
	}
	
	
}

