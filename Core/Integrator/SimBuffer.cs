//Yves Wang @ FISH, 2015, All rights reserved
/// <summary>
/// Sim buffer.
/// The buffer hold all the data for the gpu integrator
/// </summary>

using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

namespace ParticlePhysics2D {

	public class SimBuffer {
		
		
		Simulation sim;
		int ID_PositionCache,ID_PositionRT;
		
		Mesh springMesh,angleMesh,verletMesh;
		int width,height;// the rendertexture size for the particle data structure
		
		RenderTexture StateRT;
		public RenderTexture PositionRT;
		public RenderTexture PositionOldRT;
		
		RenderTexture TemporaryPositionRT {
			get {
				return RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.RGFloat);
			}
		}
		
		//for readin from gpu to cpu
		private Texture2D tempPos;
		private Rect tempRect;
		
		Vector2[] particleUV;
		MaterialPropertyBlock springMpb,angleMpb,verletMpb;
		CommandBuffer cBuffer;
		
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
			this.ID_PositionRT = Shader.PropertyToID("_PositionRT");
			Init();
		}
		
		/// <summary>
		/// If we need this GPU solver, we should re-allocate in init;
		/// </summary>
		public void Init() {
		
			InitializeRT(ref PositionRT);
			InitializeRT(ref StateRT);
			InitializeRT(ref PositionOldRT);
			
			SendToGPU_ParticleState();
			
			//initialize the temporary texture2d for exchanging position bewteen cpu and gpu
			if (tempPos==null) {
				tempPos = new Texture2D (width,height,TextureFormat.RGBAFloat,false);
				tempPos.filterMode = FilterMode.Point;
				tempPos.anisoLevel = 0;
			}
			
			//initialize the rect for readpixels
			tempRect = new Rect (0f,0f,(float)width,(float)height);
			
			
			SendToGPU_ParticlePosition();
			
			Graphics.Blit(PositionRT,PositionOldRT);
			
			GenerateParticleUV();
			GenerateSpringMesh();
			GenerateAngleMesh();
			GenerateVerletMesh();
			
			cBuffer = new CommandBuffer ();
			cBuffer.name = "SimBufferCommand";
			
			springMpb = new MaterialPropertyBlock ();
			springMpb.SetTexture("_StateRT",StateRT);
			springMpb.AddFloat("_SpringConstant",sim.springConstant);
			
			angleMpb = new MaterialPropertyBlock ();
			angleMpb.SetTexture("_StateRT",StateRT);
			angleMpb.AddFloat("_AngleRelaxPercent",sim.angleRelaxPercent);
			
			verletMpb = new MaterialPropertyBlock ();
			verletMpb.SetFloat("_Damping",sim.damping);
			//Debug.Break();
		}
		
		void InitializeRT (ref RenderTexture RT) {
			
			if (RT==null) {
				RT = new RenderTexture (width,height,0,RenderTextureFormat.RGFloat);
				RT.Create();
			}
			else {
				if (RT.IsCreated() == false) RT.Create();
				else RT.DiscardContents();
			}
		}
		
		Mesh PointMesh ( int n) {
			Mesh o = new Mesh ();
			Vector3[] vert = new Vector3[n];
			o.vertices = vert;
			int[] ind = new int[n];
			for (int i=0;i<n;i++) {
				ind[i] = i;
			}
			o.SetIndices(ind,MeshTopology.Points,0);
			return o;
		}
		
		
		void GenerateParticleUV () {
			particleUV  = new Vector2[sim.numberOfParticles()];
			int counter = 0;
			Vector2 uv;
			for (int i=0;i<height;i++) {
				for (int j=0;j<width;j++) {
					if (counter<sim.numberOfParticles()) {
						uv.x = (float)j / (float)width;
						uv.y = (float)i / (float)height;
						particleUV[counter] = uv;
						counter++;
					} 
				}
			}
		}
		
		void GenerateSpringMesh () {
			int springCount = sim.numberOfSprings();
			springMesh = PointMesh(springCount);
			Vector3[] vtc = new Vector3[springCount];
			Vector2[] uv = new Vector2[springCount];
			for (int i=0;i<springCount;i++) {
				//partciel a
				Vector2 paUV = particleUV[ sim.getParticleIndex(sim.getSpring(i).ParticleA) ];
				vtc[i].x = paUV.x;
				vtc[i].y = paUV.y;
				vtc[i].z = sim.getSpring(i).restLength2;
				//particle b
				Vector2 pbUV = particleUV[ sim.getParticleIndex(sim.getSpring(i).ParticleB) ];
				uv[i] = pbUV;
			}
			springMesh.vertices = vtc;
			springMesh.uv = uv;
			springMesh.UploadMeshData(true);
		}
		
		void GenerateAngleMesh () {
			angleMesh = PointMesh( sim.numberOfAngleConstraints());
			int angleCount = sim.numberOfAngleConstraints();
			
			Vector3[] vtc = new Vector3[angleCount];
			Color[] color = new Color[angleCount];
			for (int i=0;i<angleCount;i++) {
				AngleConstraint2D angleC = sim.getAngleConstraint(i);
				//partciel a
				Vector2 paUV = particleUV[ sim.getParticleIndex(angleC.ParticleA) ];
				vtc[i].x = paUV.x;
				vtc[i].y = paUV.y;
				
				//fixed angle
				vtc[i].z = angleC.angle_Fixed;
				//particle b and m
				Vector2 pbUV = particleUV[ sim.getParticleIndex(angleC.ParticleB) ];
				Vector2 pmUV = particleUV[ sim.getParticleIndex(angleC.ParticleM) ];
				color[i] = new Color (pbUV.x,pbUV.y,pmUV.x,pmUV.y);
			}
			angleMesh.vertices = vtc;
			angleMesh.colors = color;
			angleMesh.UploadMeshData(true);
			
		}
		
		//geenrate a full screen quad for manual blit in CommandBuffer
		void GenerateVerletMesh () {
			if (verletMesh) verletMesh.Clear();
			else verletMesh = new Mesh ();
			Vector3[] vtc = new Vector3[4];
			vtc[0] = new Vector3 (-1f,1f,0f);
			vtc[1] = new Vector3 (1f,1f,0f);
			vtc[2] = new Vector3 (1f,-1f,0f);
			vtc[3] = new Vector3 (-1f,-1f,0f);
			verletMesh.vertices = vtc;
			int[] idc = new int[4] {0,1,2,3};
			verletMesh.SetIndices(idc,MeshTopology.Quads,0);
			Vector2[] uv = new Vector2[4];
			uv[0] = new Vector2 (0f,1f); 
			uv[1] = new Vector2 (1f,1f);
			uv[2] = new Vector2 (1f,0f);
			uv[3] = new Vector2 (0f,0f);
			verletMesh.uv = uv;
			verletMesh.UploadMeshData(true);
		}
		
		#endregion
		
		#region Update
		public void Update(Material springMtl,Material angleMtl,Material verletMtl) {
			
			//in the previous step we have get all the data from cpu to GPU, in PositionRT
			RenderTexture pRT_spring = TemporaryPositionRT;
			RenderTexture pRT_angle = TemporaryPositionRT;
			RenderTexture pRT_verlet = TemporaryPositionRT;
			
			cBuffer.Clear();
			
			//prepare
			springMpb.SetTexture(ID_PositionRT,PositionRT);
			angleMpb.SetTexture(ID_PositionRT,pRT_spring);
			verletMpb.SetTexture(ID_PositionCache,PositionOldRT);
			verletMpb.SetTexture(ID_PositionRT,pRT_angle);
			
			//spring
			cBuffer.Blit(PositionRT as Texture,pRT_spring);
			cBuffer.SetRenderTarget(pRT_spring);
			cBuffer.DrawMesh(springMesh,Matrix4x4.identity,springMtl,0,-1,springMpb);
			
//			//angle
//			cBuffer.SetRenderTarget(pRT_angle);
//			cBuffer.DrawMesh(angleMesh,Matrix4x4.identity,angleMtl,0,-1,angleMpb);
//			
//			//verlet
//			cBuffer.SetRenderTarget(pRT_verlet);
//			cBuffer.DrawMesh(verletMesh,Matrix4x4.identity,verletMtl,0,-1,verletMpb);
			
			Graphics.ExecuteCommandBuffer(cBuffer);
			
			PositionRT.DiscardContents();
			PositionOldRT.DiscardContents();
			Graphics.Blit(pRT_spring,PositionRT);
			Graphics.Blit(pRT_angle,PositionOldRT);
			
			RenderTexture.ReleaseTemporary(pRT_verlet);
			RenderTexture.ReleaseTemporary(pRT_spring);
			RenderTexture.ReleaseTemporary(pRT_angle);
			
			//Debug.Break();
		}
		#endregion
		
		#region Data transfer between cpu and gpu
		//this will get the particles state into StateRT
		void SendToGPU_ParticleState ( ) {
			
			
			Texture2D state2d = new Texture2D (width,height,TextureFormat.RHalf,false,false);
			state2d.filterMode = FilterMode.Point;
			state2d.anisoLevel = 0;
			Color[] pc = new Color[width * height];
			int counter = 0;
			for (int i=0;i<height;i++) {
				for (int j=0;j<width;j++) {
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
				for (int j=0;j<width;j++) {
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
			
			//if (PositionRT != null ) RenderTexture.ReleaseTemporary(PositionRT);
			//PositionRT = TemporaryPositionRT;
			
			Graphics.Blit(tempPos,PositionRT);
			
			dbgrt = GameObject.FindObjectOfType<DebugRT>();
			dbgrt.rt = PositionRT;
		}
		
		DebugRT dbgrt;
		
		//this will transfer data in PositionRT into the particle list
		public void SendToCPU_ParticlePosition() {
		
			RenderTexture.active = PositionRT;
			tempPos.ReadPixels(tempRect,0,0,false);
			tempPos.Apply(false);
			RenderTexture.active = null;
			PositionRT.DiscardContents();
			
			Color[] pc;
			pc = tempPos.GetPixels();
			int counter = 0;
			for (int i=0;i<height;i++) {
				for (int j=0;j<width;j++) {
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
			PositionRT.Release();
			StateRT.Release();
			RenderTexture.ReleaseTemporary(PositionOldRT);
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

