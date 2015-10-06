//Yves Wang @ FISH, 2015, All rights reserved
/// <summary>
/// Sim buffer.
/// The buffer hold all the data for the gpu integrator
/// </summary>

using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

namespace ParticlePhysics2D {
	
	public partial class SimBuffer {
		
		
		public Simulation sim;
		int ID_PositionCache,ID_PositionRT;
		
		public static Mesh quadMesh;
		
		int width,height;// the rendertexture size for the particle data structure
		
		int curr = 0,next = 1;
		
		public RenderTexture PositionRT;
		RenderTexture[] PositionOldRT;
		RenderTexture springRT;
		
		SimBuffer_Spring simSpring;
		SimBuffer_Angle simAngle;
		
		RenderTexture TemporaryPositionRT {
			get {
				RenderTexture rt = RenderTexture.GetTemporary(width,height,0,RTFormat.RG);
				rt.filterMode = FilterMode.Point;
				return rt;
			}
		}
		
		//for readin from gpu to cpu
		private Texture2D tempPos;
		private Rect tempRect;
		
		public Vector2[] particleUV;
		MaterialPropertyBlock verletMpb;
		CommandBuffer cBuffer;
		
		
		
		#region Ctor
		
		public static SimBuffer Create(Simulation sim) {
			int x,y;
			float u;
			if  (sim.maxAngleConvergenceID <=0 || sim.maxAngleConvergenceID <= 0) {
				Debug.LogError("The simualtion does not have correct convergence group, GPU solver cannot be created!");
				return null;
			}
			if (!SimBuffer.GetTexDimension(sim.numberOfParticles(),out x,out y,out u)) {
				Debug.LogError("Cannot create GPU rendertexture with wrong dimension");
				return null;
			}
			return new SimBuffer (x,y,sim);
		}
		
		//create the simulation gpu buffer
		private SimBuffer(int x,int y,Simulation sim) {
			this.width = x;
			this.height = y;
			this.sim = sim;
			this.ID_PositionCache = Shader.PropertyToID("_PositionCache");
			this.ID_PositionRT = Shader.PropertyToID("_PositionRT");
			Init();
		}
		
		/// <summary>
		/// If we need this GPU solver, we should re-allocate in init;
		/// </summary>
		public void Init() {
		
			PositionOldRT = new RenderTexture[2];
			Init_RT(sim.numberOfParticles(),RTFormat.RG,ref PositionRT);
			Init_RT(sim.numberOfParticles(),RTFormat.RG,ref PositionOldRT[curr]);
			Init_RT(sim.numberOfParticles(),RTFormat.RG,ref PositionOldRT[next]);
			
			//initialize the temporary texture2d for exchanging position bewteen cpu and gpu
			tempPos = new Texture2D (width,height,TextureFormat.RGBAFloat,false);
			tempPos.filterMode = FilterMode.Point;
			tempRect = new Rect (0f,0f,(float)width,(float)height);//initialize the rect for readpixels
			particlePositionColor = new Color[width * height];
			
			SendToGPU_ParticlePosition();
			Graphics.Blit(PositionRT,PositionOldRT[curr]);
			
			GenerateParticleUV();
			GenerateQuadMesh();
			
			simSpring = new SimBuffer_Spring (sim,this.particleUV,width,height);
			simAngle = new SimBuffer_Angle (sim,this.particleUV,width,height);
			
			cBuffer = new CommandBuffer ();
			cBuffer.name = "SimBufferCommand";
			cBuffer.Clear();
			
			verletMpb = new MaterialPropertyBlock ();
		}
		#endregion
		
		#region Update
		
	

		public void Update() {
			
			verletMpb.SetTexture(ID_PositionRT,PositionOldRT[next]);
			verletMpb.SetTexture(ID_PositionCache,PositionOldRT[curr]);
			verletMpb.SetFloat("_Damping",sim.Settings.damping);
			
			springRT = TemporaryPositionRT;
			
			//cBuffer.Blit(PositionRT as Texture , PositionOldRT[next]);
			
			simSpring.Blit(ref cBuffer,ref PositionRT,ref springRT);
			
			//cBuffer.Blit(springRT as Texture , DebugRT.Instance.springRT);
			
			simAngle.Blit(ref cBuffer, ref springRT , ref PositionOldRT[next]);
			
			//cBuffer.Blit(PositionOldRT[next] as Texture , DebugRT.Instance.SendToGPU_PositionRT);
			
			//verlet
			cBuffer.SetRenderTarget(PositionRT);
			cBuffer.ClearRenderTarget(false,true,Color.clear);//no harm
			cBuffer.DrawMesh(quadMesh,Matrix4x4.identity,GPUVerletIntegrator.verletMtl,0,-1,verletMpb);
			
			//cBuffer.Blit(PositionRT as Texture , DebugRT.Instance.SendToCPU_PositionRT);
			
			Graphics.ExecuteCommandBuffer(cBuffer);
			
			simSpring.ReleaseTempRT();
			simAngle.ReleaseTempRT();
			RenderTexture.ReleaseTemporary(springRT);
			
			int temp = next;
			next = curr;
			curr = temp;
			cBuffer.Clear();
			
		}
		#endregion
		
		/// <summary>
		/// If the simualtion goes out of scope, we can release the hardware resources manually
		/// </summary>
		public void Release() {
			PositionRT.Release();
			RenderTexture.ReleaseTemporary(PositionOldRT[curr]);
			RenderTexture.ReleaseTemporary(PositionOldRT[next]);
			Extension.ObjDestroy(tempPos);
		}
	}
	
	//
	public static class RTFormat {
		public static bool isHighPrecisionOn = true;
		public static RenderTextureFormat RG = (isHighPrecisionOn) ? RenderTextureFormat.RGFloat : RenderTextureFormat.RGHalf;
		public static RenderTextureFormat ARGB = (isHighPrecisionOn) ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.ARGBHalf;
		public static RenderTextureFormat R = (isHighPrecisionOn) ? RenderTextureFormat.RFloat : RenderTextureFormat.RHalf;
		public static RenderTextureFormat R8 = RenderTextureFormat.R8;
	}
	
	
	
}

