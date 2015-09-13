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
		
		Mesh springMesh,angleMesh,quadMesh;
		int width,height;// the rendertexture size for the particle data structure
		
		int curr = 0,next = 1;
		
		RenderTexture StateRT;
		public RenderTexture PositionRT;
		public RenderTexture[] PositionOldRT;
		
		public RenderTexture SpringRT;//once initialized, we only read from springrt for querying the two end's uv
		public RenderTexture SpringRT_RestLength;//only store RFloat as spring rest length
		public RenderTexture AngleRT_AB,AngleRT_M;//store the end particle position uv;AngleRT_AB = particle a and b;AngleRT_M = particle m and fixed angle		
		RenderTexture TemporaryPositionRT {
			get {
				return RenderTexture.GetTemporary(width,height,0,RTFormat.RG);
			}
		}
		RenderTexture TemporaryDeltaRT {
			get {return RenderTexture.GetTemporary(SpringRT.width,SpringRT.height,0,RTFormat.RG);}
		}
		
		//for readin from gpu to cpu
		private Texture2D tempPos;
		private Rect tempRect;
		
		Vector2[] particleUV;
		MaterialPropertyBlock springMpb,angleMpb,verletMpb;
		MaterialPropertyBlock springDeltaMpb,angleDeltaMpb;
		CommandBuffer cBuffer;
		
		static class RTFormat {
			public static RenderTextureFormat RG = RenderTextureFormat.RGFloat;
			public static RenderTextureFormat ARGB = RenderTextureFormat.ARGBFloat;
			public static RenderTextureFormat R = RenderTextureFormat.RFloat;
			public static RenderTextureFormat R8 = RenderTextureFormat.R8;
		}
		
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
		
			PositionOldRT = new RenderTexture[2];
			Init_RT(sim.numberOfParticles(),RTFormat.RG,ref PositionRT);
			Init_RT(sim.numberOfParticles(),RTFormat.RG,ref PositionOldRT[curr]);
			Init_RT(sim.numberOfParticles(),RTFormat.RG,ref PositionOldRT[next]);
			Init_RT(sim.numberOfParticles(),RTFormat.R8,ref StateRT);
			Init_RT(sim.numberOfSprings(),RTFormat.ARGB, ref SpringRT);
			Init_RT(sim.numberOfSprings(),RTFormat.R, ref SpringRT_RestLength);
			Init_RT(sim.numberOfAngleConstraints(),RTFormat.ARGB, ref AngleRT_AB);
			Init_RT(sim.numberOfAngleConstraints(),RTFormat.ARGB, ref AngleRT_M);
			
			
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
			
			Graphics.Blit(PositionRT,PositionOldRT[curr]);
			
			PositionRT.MarkRestoreExpected();
			PositionOldRT[curr].MarkRestoreExpected();
			
			GenerateParticleUV();
			
			GenerateSpringMesh();
			GenerateSpringRT();
			
			GenerateAngleMesh();
			GenerateAngleRT();
			
			GenerateQuadMesh();
			
			
			cBuffer = new CommandBuffer ();
			cBuffer.name = "SimBufferCommand";
			
			springDeltaMpb = new MaterialPropertyBlock ();
			springDeltaMpb.SetFloat("_SpringConstant",sim.springConstant);
			springDeltaMpb.SetTexture("_PositionRT",PositionRT);
			springDeltaMpb.SetTexture("_SpringRT",SpringRT);
			springDeltaMpb.SetTexture("_RestLength2RT",SpringRT_RestLength);
			
			springMpb = new MaterialPropertyBlock ();
			springMpb.SetTexture("_StateRT",StateRT);
			
			angleDeltaMpb.SetFloat("_AngleRelaxPercent",sim.angleRelaxPercent);
			angleDeltaMpb.SetTexture("_PositionRT",PositionRT);
			angleDeltaMpb.SetTexture("_AngleRT_AB",AngleRT_AB);
			angleDeltaMpb.SetTexture("_AngleRT_M",AngleRT_M);
			
			angleMpb = new MaterialPropertyBlock ();
			angleMpb.SetTexture("_StateRT",StateRT);
			
			verletMpb = new MaterialPropertyBlock ();
			verletMpb.SetFloat("_Damping",sim.damping);
			//Debug.Break();
		}
		
		
		
		void Init_RT(int numberOfElements,RenderTextureFormat format,ref RenderTexture RT) {
			int x,y;
			float u;
			if ( GetTexDimension( numberOfElements,out x,out y,out u)) {
				if (RT==null) {
					RT = new RenderTexture (x,y,0,format);
					RT.Create();
				}
				else {
					if (RT.width!=x || RT.height!=y) {
						Extension.ObjDestroy(RT);
						RT = new RenderTexture (x,y,0,format);
						RT.Create();
					} else {
						if (RT.IsCreated() == false) RT.Create();
						else RT.DiscardContents();
					}
				}
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
			Color[] color = new Color[springCount];
			//Vector2[] uv = new Vector2[springCount];
//			for (int i=0;i<springCount;i++) {
//				continue;
//				//partciel a
//				Vector2 paUV = particleUV[ sim.getParticleIndex(sim.getSpring(i).ParticleA) ];
//				vtc[i].x = paUV.x;
//				vtc[i].y = paUV.y;
//				vtc[i].z = sim.getSpring(i).restLength2;
//				//particle b
//				Vector2 pbUV = particleUV[ sim.getParticleIndex(sim.getSpring(i).ParticleB) ];
//				//uv[i] = pbUV;
//			}
			int count = 0;
			for (int i=0;i<SpringRT.height;i++) {
				for (int j=0;j<SpringRT.width;j++) {
					if (count<springCount) {
						vtc[count].x = j / SpringRT.width;
						vtc[count].y = i/SpringRT.height;
						vtc[count].z = 0f;
						Spring2D sp = sim.getSpring(count);
						int a = sim.getParticleIndex(sp.ParticleA);
						int b = sim.getParticleIndex(sp.ParticleB);
						color[count] = new Color (particleUV[a].x,particleUV[a].y,particleUV[b].x,particleUV[b].y);
					}
					count ++;
				}
			}
			
			springMesh.vertices = vtc;
			//springMesh.uv = uv;
			springMesh.UploadMeshData(true);
		}
		
		void GenerateSpringRT () {
			Texture2D tempSpringRT;
			
			tempSpringRT = new Texture2D (SpringRT.width,SpringRT.height,TextureFormat.RGBAFloat,false);
			tempSpringRT.filterMode = FilterMode.Point;
			tempSpringRT.anisoLevel = 0;
			
			int springNum = sim.numberOfSprings();
			int count = 0;
			Color[] abpos = new Color[SpringRT.width * SpringRT.height];
			Color[] restlength = new Color[abpos.Length];
			for (int i=0;i<SpringRT.height;i++) {
				for (int j=0;j<SpringRT.width;j++) {
					if (count < springNum) {
						Spring2D sp = sim.getSpring(count);
						int pa = sim.getParticleIndex(sp.ParticleA);
						int pb = sim.getParticleIndex(sp.ParticleB);
						abpos[count] = new Color (particleUV[pa].x,particleUV[pa].y,particleUV[pb].x,particleUV[pb].y);
						restlength[count] = new Color (sp.restLength2,0f,0f);
					} else {
						abpos[count] = Color.clear;
						restlength[count] = Color.clear;
					}
					count++;
				}
			}
			tempSpringRT.SetPixels(abpos);
			tempSpringRT.Apply(false);
			Graphics.Blit(tempSpringRT,SpringRT);
			tempSpringRT.SetPixels(restlength);
			tempSpringRT.Apply(false);
			Graphics.Blit(tempSpringRT,SpringRT_RestLength);
		}
		
		void GenerateAngleMesh () {
			angleMesh = PointMesh( sim.numberOfAngleConstraints());
			int angleCount = sim.numberOfAngleConstraints();
			
			Vector3[] vtc = new Vector3[angleCount];
			Vector2[] uv = new Vector2[angleCount];//pa uv
			Color[] color = new Color[angleCount];//pb uv pm uv
			//Color[] color = new Color[angleCount];
//			for (int i=0;i<angleCount;i++) {
//				AngleConstraint2D angleC = sim.getAngleConstraint(i);
//				//partciel a
//				Vector2 paUV = particleUV[ sim.getParticleIndex(angleC.ParticleA) ];
//				vtc[i].x = paUV.x;
//				vtc[i].y = paUV.y;
//				
//				//fixed angle
//				vtc[i].z = angleC.angle_Fixed;
//				//particle b and m
//				Vector2 pbUV = particleUV[ sim.getParticleIndex(angleC.ParticleB) ];
//				Vector2 pmUV = particleUV[ sim.getParticleIndex(angleC.ParticleM) ];
//				color[i] = new Color (pbUV.x,pbUV.y,pmUV.x,pmUV.y);
//			}
			int count = 0;
			for (int i=0;i<AngleRT_AB.height;i++) {
				for (int j=0;j<AngleRT_AB.width;j++) {
					if (count<angleCount) {
						vtc[count].x = j / AngleRT_AB.width;
						vtc[count].y = i / AngleRT_AB.height;
						vtc[count].z = 0f;
						AngleConstraint2D angleC = sim.getAngleConstraint(i);
						Vector2 paUV = particleUV[ sim.getParticleIndex(angleC.ParticleA) ];
						uv[count].x = paUV.x;
						uv[count].y = paUV.y;
						//particle b and m
						Vector2 pbUV = particleUV[ sim.getParticleIndex(angleC.ParticleB) ];
						Vector2 pmUV = particleUV[ sim.getParticleIndex(angleC.ParticleM) ];
						color[count] = new Color (pbUV.x,pbUV.y,pmUV.x,pmUV.y);
						
					}
					count++;
				}
			}
			angleMesh.vertices = vtc;//vertex.xy = pa,vertex.z = fixed angle; color.rg = pb;color.ba = pm;
			angleMesh.colors = color;//particle b and m
			angleMesh.uv = uv;//particle a
			angleMesh.UploadMeshData(true);
			
		}
		
		void GenerateAngleRT() {
			Texture2D tempAngleRT;
			tempAngleRT = new Texture2D (AngleRT_AB.width,AngleRT_AB.height,TextureFormat.RGBAFloat,false);
			tempAngleRT.filterMode = FilterMode.Point;
			tempAngleRT.anisoLevel = 0;
			
			int angleNum = sim.numberOfAngleConstraints();
			int count = 0;
			Color[] posab = new Color[AngleRT_AB.width * AngleRT_AB.height];
			Color[] posm = new Color[AngleRT_AB.width * AngleRT_AB.height];
			for (int i=0;i<AngleRT_AB.height;i++) {
				for (int j=0;j<AngleRT_AB.width;j++) {
					if (count<angleNum) {
						AngleConstraint2D angle = sim.getAngleConstraint(count);
						int a = sim.getParticleIndex(angle.ParticleA);
						int b = sim.getParticleIndex(angle.ParticleB);
						int m = sim.getParticleIndex(angle.ParticleM);
						posab[count] = new Color (particleUV[a].x,particleUV[a].y,particleUV[b].x,particleUV[b].y);
						posm[count] = new Color (particleUV[m].x,particleUV[m].y,angle.angle_Fixed);
					} else {
						posab[count] = posm[count] = Color.clear;
					}
					count++;
				}
			}
			tempAngleRT.SetPixels(posab);
			tempAngleRT.Apply(false);
			Graphics.Blit(tempAngleRT,AngleRT_AB);
			tempAngleRT.SetPixels(posm);
			Graphics.Blit(tempAngleRT,AngleRT_M);
		}
		
		//geenrate a full screen quad for manual blit in CommandBuffer
		//because we need to set mpb when drawing mesh
		void GenerateQuadMesh () {
			if (quadMesh) quadMesh.Clear();
			else quadMesh = new Mesh ();
			Vector3[] vtc = new Vector3[4];
			vtc[0] = new Vector3 (-1f,1f,0f);
			vtc[1] = new Vector3 (1f,1f,0f);
			vtc[2] = new Vector3 (1f,-1f,0f);
			vtc[3] = new Vector3 (-1f,-1f,0f);
			quadMesh.vertices = vtc;
			int[] idc = new int[4] {0,1,2,3};
			quadMesh.SetIndices(idc,MeshTopology.Quads,0);
			Vector2[] uv = new Vector2[4];
			uv[0] = new Vector2 (0f,1f); 
			uv[1] = new Vector2 (1f,1f);
			uv[2] = new Vector2 (1f,0f);
			uv[3] = new Vector2 (0f,0f);
			quadMesh.uv = uv;
			quadMesh.UploadMeshData(true);
		}
		
		#endregion
		
		#region Update
		
		// 
		// 1. angle delta :
		// 
		public void Update(Material springMtl, Material springDeltaMtl, Material angleMtl, Material angleDeltaMtl, Material verletMtl) {
			
//			//in the previous step we have get all the data from cpu to GPU, in PositionRT
//			RenderTexture pRT_spring = TemporaryPositionRT;
//			RenderTexture pRT_angle = TemporaryPositionRT;
//			RenderTexture pRT_verlet = TemporaryPositionRT;
//			
//			cBuffer.Clear();
//			
//			//prepare
//			springMpb.SetTexture(ID_PositionRT,PositionRT);
//			angleMpb.SetTexture(ID_PositionRT,pRT_spring);
//		
//			
//			//spring
//			cBuffer.Blit(PositionRT as Texture,pRT_spring);
//			cBuffer.SetRenderTarget(pRT_spring);
//			cBuffer.DrawMesh(springMesh,Matrix4x4.identity,springMtl,0,-1,springMpb);
//			
////			//angle
////			cBuffer.SetRenderTarget(pRT_angle);
////			cBuffer.DrawMesh(angleMesh,Matrix4x4.identity,angleMtl,0,-1,angleMpb);
////			
////			//verlet
////			cBuffer.SetRenderTarget(pRT_verlet);
////			cBuffer.DrawMesh(verletMesh,Matrix4x4.identity,verletMtl,0,-1,verletMpb);
//			
//			Graphics.ExecuteCommandBuffer(cBuffer);
//			
//			PositionRT.DiscardContents();
//			//PositionOldRT.DiscardContents();
//			Graphics.Blit(pRT_spring,PositionRT);
//			Graphics.Blit(pRT_angle,PositionOldRT);
//			
//			RenderTexture.ReleaseTemporary(pRT_verlet);
//			RenderTexture.ReleaseTemporary(pRT_spring);
//			RenderTexture.ReleaseTemporary(pRT_angle);
			
			//Debug.Break();
			//======================================================================
			
			RenderTexture springDeltaRT = TemporaryDeltaRT;
			RenderTexture angleDeltaRT = TemporaryDeltaRT;
			
			verletMpb.SetTexture(ID_PositionRT,PositionOldRT[next]);
			verletMpb.SetTexture(ID_PositionCache,PositionOldRT[curr]);
			
			cBuffer.Clear();
			
			// 1. spring delta : springdeltaMtl -> settexture PositionRT
			// 2. drawmesh springrt => deltaRT
			// 3. first sample springrt, get the uv for pa and pb,then sample positionrt get the position of pa and pb
			// 4. then sample the restlengthrt, to get the restlength2;
			// 5. then output the delta to the deltaRT
			//spring delta
			cBuffer.SetRenderTarget(springDeltaRT);
			cBuffer.DrawMesh(quadMesh,Matrix4x4.identity,springDeltaMtl,0,-1,springDeltaMpb);
			
			//apply spring delta
			// 1. draw mesh of the spring pt
			// 2. get the delta from prev result by sampling SpringDeltaRT
			// 3. move spring pt to particle a's uv then output delta by blending with existing positionRT
			// 4. move to b
			cBuffer.SetRenderTarget(PositionRT);
			cBuffer.DrawMesh(springMesh,Matrix4x4.identity,springMtl,0,0,springMpb);//particle a = pass 0
			cBuffer.DrawMesh(springMesh,Matrix4x4.identity,springMtl,0,1,springMpb);//particle b = pass 1
			
			//angle delta
			cBuffer.SetRenderTarget(angleDeltaRT);
			cBuffer.DrawMesh(quadMesh,Matrix4x4.identity,angleDeltaMtl,0,-1,angleDeltaMpb);
			
			//apply angle delta
			cBuffer.SetRenderTarget(PositionOldRT[next]);
			cBuffer.ClearRenderTarget(false,true,Color.clear);
			cBuffer.DrawMesh(angleMesh,Matrix4x4.identity,angleMtl,0,0,angleMpb);//pa
			cBuffer.DrawMesh(angleMesh,Matrix4x4.identity,angleMtl,0,1,angleMpb);//pb
			cBuffer.DrawMesh(angleMesh,Matrix4x4.identity,angleMtl,0,2,angleMpb);//pm
			
			//verlet
			cBuffer.SetRenderTarget(PositionRT);
			cBuffer.ClearRenderTarget(false,true,Color.clear);
			cBuffer.DrawMesh(quadMesh,Matrix4x4.identity,verletMtl,0,-1,verletMpb);
			
			Graphics.ExecuteCommandBuffer(cBuffer);
			RenderTexture.ReleaseTemporary(springDeltaRT);
			RenderTexture.ReleaseTemporary(angleDeltaRT);
			
			int temp = next;
			next = curr;
			curr = temp;
			
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
			RenderTexture.ReleaseTemporary(PositionOldRT[curr]);
			RenderTexture.ReleaseTemporary(PositionOldRT[next]);
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

