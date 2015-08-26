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
		Simulation sim;
		int ID_PositionCache;
		int current = 0,next = 1;//for position tex
		int width,height;// the rendertexture size for the particle data structure
		
		public RenderTexture PositionRT {get;set;}
		
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
			for (int i=0;i<2;i++) {
				poRT[i] = new RenderTexture (x,y,0,RenderTextureFormat.RGFloat);
				poRT[i].useMipMap = false;
				poRT[i].anisoLevel = 1;
				poRT[i].filterMode = FilterMode.Point;
			}
			
			Init();
			
		}
		
		/// <summary>
		/// If we need this GPU solver, we should init it with resources allocating
		/// </summary>
		public void Init() {
			//get all the current data to rt
			Texture2D pos = new Texture2D (width,height,TextureFormat.RGFloat,false);
			pos.filterMode = FilterMode.Point;
			pos.anisoLevel = 0;
			
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
			
			pos.SetPixels(pc);
			pos.Apply(false);
			
			for (int i=0;i<2;i++) {
				if (!poRT[i].IsCreated()) poRT[i].Create();
				Graphics.Blit(pos,poRT[i]);
			}
			
			if (PositionRT) RenderTexture.ReleaseTemporary(PositionRT);
			PositionRT = RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.RGFloat);
			
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
		/// Verlet the result of this frame and the prev frame
		/// </summary>
		public void Verlet (Material mtl,int pass = -1) {
			mtl.SetTexture(ID_PositionCache,poRT[current]);
			PositionRT = RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.RGFloat);
			Graphics.Blit(poRT[next],PositionRT,mtl,pass);
			RenderTexture.ReleaseTemporary(poRT[current]);
			current ++; current %= 2;
			next ++; next %= 2;
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
		}
		
		public void Destroy() {
			Extension.ObjDestroy(PositionRT);
			Extension.ObjDestroy(poRT[0]);
			Extension.ObjDestroy(poRT[1]);
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

