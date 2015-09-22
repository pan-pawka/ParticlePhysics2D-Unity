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
				RenderTexture rt = RenderTexture.GetTemporary(width,height,0,RTFormat.RG);
				rt.filterMode = FilterMode.Point;
				return rt;
			}
		}
		RenderTexture TemporaryDeltaRT {
			get {
				RenderTexture rt = RenderTexture.GetTemporary(SpringRT.width,SpringRT.height,0,RTFormat.RG);
				rt.filterMode = FilterMode.Point;
				return rt;
			}
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
				//tempPos.anisoLevel = 0;
			}
			
			//initialize the rect for readpixels
			tempRect = new Rect (0f,0f,(float)width,(float)height);
			
			
			SendToGPU_ParticlePosition();
			
			Graphics.Blit(PositionRT,PositionOldRT[curr]);
			
			PositionRT.MarkRestoreExpected();
			//PositionOldRT[curr].MarkRestoreExpected();
			
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
			
			angleDeltaMpb = new MaterialPropertyBlock ();
			angleDeltaMpb.SetFloat("_AngleRelaxPercent",sim.angleRelaxPercent);
			angleDeltaMpb.SetTexture("_PositionRT",PositionRT);
			angleDeltaMpb.SetTexture("_AngleRT_AB",AngleRT_AB);
			angleDeltaMpb.SetTexture("_AngleRT_M",AngleRT_M);
			
			angleMpb = new MaterialPropertyBlock ();
			angleMpb.SetTexture("_StateRT",StateRT);
			angleMpb.SetTexture("_PositionRT",PositionRT);
			
			verletMpb = new MaterialPropertyBlock ();
			verletMpb.SetFloat("_Damping",sim.damping);
			//Debug.Break();
		}
		
		
		
		void Init_RT(int numberOfElements,RenderTextureFormat format,ref RenderTexture RT) {
			int x,y;
			float u;
			if ( GetTexDimension( numberOfElements,out x,out y,out u)) {
				//Debug.Log("Init RT " + " X = " + x + " Y = " + y);
				if (RT==null) {
					RT = new RenderTexture (x,y,0,format);
					RT.filterMode = FilterMode.Point;
					RT.Create();
				}
				else {
					if (RT.width!=x || RT.height!=y) {
						Extension.ObjDestroy(RT);
						RT = new RenderTexture (x,y,0,format);
						RT.filterMode = FilterMode.Point;
						RT.Create();
					} else {
						if (RT.IsCreated() == false) RT.Create();
						else RT.DiscardContents();
					}
				}
			} else {
				Debug.LogError("Cannot init RT for " + RT + " with right dimension");
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
			float halfPixelWidth = 0.5f / width;
			float halfPixelHeight = 0.5f / height;
			Vector2 uv;
			for (int i=0;i<height;i++) {
				for (int j=0;j<width;j++) {
					if (counter<sim.numberOfParticles()) {
						uv.x = (float)j / (float)width + halfPixelWidth;
						uv.y = (float)i / (float)height + halfPixelHeight;
						particleUV[counter] = uv;
						//Debug.Log(uv);
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
		
			int count = 0;
			float halfPixelOffsetX = 0.5f / SpringRT.width;
			float halfPixelOffsetY = 0.5f / SpringRT.height;
			
			for (int i=0;i<SpringRT.height;i++) {
				for (int j=0;j<SpringRT.width;j++) {
					if (count<springCount) {
						vtc[count].x = (float)j / (float)SpringRT.width + halfPixelOffsetX;
						vtc[count].y = (float)i / (float)SpringRT.height + halfPixelOffsetY;
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
			springMesh.colors = color;
			
			//springMesh.UploadMeshData(true);
		}
		
		void GenerateSpringRT () {
			Texture2D tempSpringRT,tempLength2RT;
			
			tempSpringRT = new Texture2D (SpringRT.width,SpringRT.height,TextureFormat.RGBAFloat,false);
			tempSpringRT.filterMode = FilterMode.Point;
			
			tempLength2RT = new Texture2D (SpringRT.width,SpringRT.height,TextureFormat.RFloat,false);
			tempLength2RT.filterMode = FilterMode.Point;
			
			//tempSpringRT.anisoLevel = 0;
			
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
			
			//debug spring rt
			DebugRT.Instance.DebugSpringRT(SpringRT);
			
			
			tempLength2RT.SetPixels(restlength);
			tempLength2RT.Apply(false);
			Graphics.Blit(tempLength2RT,SpringRT_RestLength);
			
			//---debug rest length rt
			float maxL = 0;
			for (int i=0;i<restlength.Length;i++) {
				if (restlength[i].r > maxL) maxL = restlength[i].r;
			}
			
			for (int i=0;i<restlength.Length;i++) {
				restlength[i].r /= maxL;
			}
			
			tempSpringRT.SetPixels(restlength);
			tempSpringRT.Apply(false);
			
			DebugRT.Instance.DebugRestLengthRT(tempSpringRT);
			//---debug rest length rt
			
//			//re-validate spring rt
//			//validate a b uv
//			Texture2D tex = new Texture2D (SpringRT.width,SpringRT.height,TextureFormat.RGBAFloat,false);
//			RenderTexture.active = SpringRT;
//			tex.ReadPixels(new Rect (0,0,SpringRT.width,SpringRT.height),0,0,false);
//			tex.Apply();
//			Color[] abuv = tex.GetPixels();
//			
//			
//			for (int i=0;i<springNum;i++) {
//				Spring2D sp = sim.getSpring(i);
//				int a = sim.getParticleIndex(sp.ParticleA);
//				int b = sim.getParticleIndex(sp.ParticleB);
//				Color temp = new Color (particleUV[a].x,particleUV[a].y,particleUV[b].x,particleUV[b].y);
//				//Debug.Log(temp - abuv[i]);
//			}
//			
//			//validate position rt
//			Texture2D texp = new Texture2D (PositionRT.width,PositionRT.height,TextureFormat.RGBAFloat,false);
//			RenderTexture.active = PositionRT;
//			texp.ReadPixels(new Rect (0,0,PositionRT.width,PositionRT.height),0,0,false);
//			texp.Apply();
//			Color[] prt = texp.GetPixels();
//			
//			for (int i=0;i<sim.numberOfParticles();i++) {
//				//Vector2 p = new Vector2 (prt[i].r,prt[i].g);
//				int x = (int)(particleUV[i].x * PositionRT.width);
//				int y = (int)(particleUV[i].y * PositionRT.height);
//				Color pr = texp.GetPixel(x,y);
//				Vector2 p = new Vector2 (pr.r,pr.g);
//				//Debug.Log(sim.getParticle(i).Position - p);
//				
//				
//			}
//			
//			//re-validate restlength2;
//			Texture2D lt = new Texture2D (SpringRT_RestLength.width,SpringRT_RestLength.height,TextureFormat.RGBAFloat,false);
//			RenderTexture.active = SpringRT_RestLength;
//			lt.ReadPixels(new Rect (0,0,SpringRT_RestLength.width,SpringRT_RestLength.height),0,0,false);
//			lt.Apply(false);
//			Color[] ltc = lt.GetPixels();
//			
//			bool iss = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat);
//			Debug.Log(iss);
//			
//			for (int i=0;i<springNum;i++) {
//				Spring2D sp = sim.getSpring(i);
//				int ax = (int)(abuv[i].r * PositionRT.width);
//				int ay = (int)(abuv[i].g * PositionRT.height);
//				int bx = (int)(abuv[i].b * PositionRT.width);
//				int by = (int)(abuv[i].a * PositionRT.height);
//				Color pa = texp.GetPixel(ax,ay);
//				Color pb = texp.GetPixel(bx,by);
//				Vector2 p = new Vector2 (pa.r,pa.g);
//				//Debug.Log(sim.getSpring(i).ParticleA.Position - p);
//				Vector2 rl = sp.ParticleA.Position - sp.ParticleB.Position;
//				Debug.Log(sp.restLength2 - ltc[i].r);
//				
////				float l1 = (pa.r - pb.r) * (pa.r - pb.r) + (pa.g - pb.g) * (pa.g - pb.g);
////				Debug.Log(l1 - ltc[i].r);
				
			//}
			
			//Debug.Break();
			
		}
		
		void GenerateAngleMesh () {
			angleMesh = PointMesh( sim.numberOfAngleConstraints());
			int angleCount = sim.numberOfAngleConstraints();
			
			Vector3[] vtc = new Vector3[angleCount];//xy = AngleRT uv
			Vector2[] uv = new Vector2[angleCount];//pa-uv
			Color[] color = new Color[angleCount];//pb-uv pm-uv
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
			float halfPixelOffsetX = 0.5f / (float)AngleRT_AB.width;
			float halfPixelOffsetY = 0.5f / (float)AngleRT_AB.height;
			for (int i=0;i<AngleRT_AB.height;i++) {
				for (int j=0;j<AngleRT_AB.width;j++) {
					if (count<angleCount) {
						vtc[count].x = (float)j / (float)AngleRT_AB.width + halfPixelOffsetX;
						vtc[count].y = (float)i / (float)AngleRT_AB.height + halfPixelOffsetY;
						vtc[count].z = 0f;
						AngleConstraint2D angleC = sim.getAngleConstraint(count);
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
			//vertex.xy = angle rt uv, uv = pa uv, color.xy = pb uc, color.zw = pmuv;
			angleMesh.vertices = vtc;
			angleMesh.colors = color;//particle b and m
			angleMesh.uv = uv;//particle a
			//angleMesh.UploadMeshData(true);
			
		}
		
		void GenerateAngleRT() {
			Texture2D tempAngleRT_AB,tempAngleRT_M;
			tempAngleRT_AB = new Texture2D (AngleRT_AB.width,AngleRT_AB.height,TextureFormat.RGBAFloat,false);
			tempAngleRT_AB.filterMode = FilterMode.Point;
			
			tempAngleRT_M = new Texture2D  (AngleRT_AB.width,AngleRT_AB.height,TextureFormat.RGBAFloat,false);
			tempAngleRT_M.filterMode = FilterMode.Point;
			//tempAngleRT.anisoLevel = 0;
			
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
						posm[count] = new Color (particleUV[m].x,particleUV[m].y,angle.angle_Fixed,0f);
					} else {
						posab[count] = posm[count] = Color.clear;
					}
					count++;
				}
			}
			tempAngleRT_AB.SetPixels(posab);
			tempAngleRT_AB.Apply(false);
			Graphics.Blit(tempAngleRT_AB,AngleRT_AB);
			tempAngleRT_M.SetPixels(posm);
			tempAngleRT_M.Apply(false);
			Graphics.Blit(tempAngleRT_M,AngleRT_M);
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
			//quadMesh.UploadMeshData(true);
		}
		
		#endregion
		
		#region Update
		
		// 
		// 1. angle delta :
		// 
		public void Update(Material springMtl, Material springDeltaMtl, Material angleMtl, Material angleDeltaMtl, Material verletMtl) {
		
			//======================================================================
			
			RenderTexture springDeltaRT = TemporaryDeltaRT;
			RenderTexture angleDeltaRT = TemporaryDeltaRT;
			RenderTexture tempSpDeltaRT = TemporaryPositionRT;
			
			//RenderTexture tempRT = RenderTexture.GetTemporary(PositionRT.width,PositionRT.height,0,RenderTextureFormat.RGFloat);
			
			angleDeltaMpb.SetFloat("_AngleRelaxPercent",sim.angleRelaxPercent);
			verletMpb.SetTexture(ID_PositionRT,PositionOldRT[next]);
			verletMpb.SetTexture(ID_PositionCache,PositionOldRT[curr]);
			
			springDeltaMpb.SetFloat("_SpringConstant",sim.springConstant);
			springMpb.SetTexture("_SpringDeltaRT",springDeltaRT);
			angleMpb.SetTexture("_AngleDeltaRT",angleDeltaRT);
			verletMpb.SetFloat("_Damping",sim.damping);
			
			cBuffer.Clear();
			
			// 1. spring delta : springdeltaMtl -> settexture PositionRT
			// 2. drawmesh springrt => deltaRT
			// 3. first sample springrt, get the uv for pa and pb,then sample positionrt get the position of pa and pb
			// 4. then sample the restlengthrt, to get the restlength2;
			// 5. then output the delta to the deltaRT
			
			//======================================================================
			
			//spring delta
			cBuffer.SetRenderTarget(springDeltaRT);
			cBuffer.DrawMesh(quadMesh,Matrix4x4.identity,springDeltaMtl,0,-1,springDeltaMpb);
			//cBuffer.Blit(springDeltaRT as Texture,DebugRT.Instance.springDeltaRT);//debug
			
			
			//apply spring delta
			// 1. draw mesh of the spring pt
			// 2. get the delta from prev result by sampling SpringDeltaRT
			// 3. move spring pt to particle a's uv then output delta by blending with existing positionRT
			// 4. move to b
			
			cBuffer.SetRenderTarget(PositionRT);
			cBuffer.DrawMesh(springMesh,Matrix4x4.identity,springMtl,0,-1,springMpb);//Two pass: particle a = pass 0;//particle b = pass 1
			//cBuffer.Blit(PositionRT as Texture , DebugRT.Instance.spDeltaPos);//debug
			//======================================================================
			
			//======================================================================
			//angle delta 1
			cBuffer.SetRenderTarget(angleDeltaRT);
			cBuffer.DrawMesh(quadMesh,Matrix4x4.identity,angleDeltaMtl,0,-1,angleDeltaMpb);
			
			//apply angle delta
			cBuffer.Blit(PositionRT as Texture,PositionOldRT[next]);
			cBuffer.SetRenderTarget(PositionOldRT[next]);
//			cBuffer.DrawMesh(angleMesh,Matrix4x4.identity,angleMtl,0,-1,angleMpb);//pa pb and pm
			
			
			//cBuffer.Blit(PositionRT as Texture,PositionOldRT[next]);
			
			//======================================================================

//			//verlet
			cBuffer.SetRenderTarget(PositionRT);
			cBuffer.ClearRenderTarget(false,true,Color.clear);
			cBuffer.DrawMesh(quadMesh,Matrix4x4.identity,verletMtl,0,-1,verletMpb);
			
			Graphics.ExecuteCommandBuffer(cBuffer);
			
			//debug spring delta
			//DebugRT.Instance.DebugSpringDelta(springDeltaRT);
			
			
			RenderTexture.ReleaseTemporary(springDeltaRT);
			RenderTexture.ReleaseTemporary(angleDeltaRT);
			RenderTexture.ReleaseTemporary(tempSpDeltaRT);
			
			int temp = next;
			next = curr;
			curr = temp;
			
		}
		#endregion
		
		#region Data transfer between cpu and gpu
		//this will get the particles state into StateRT
		void SendToGPU_ParticleState ( ) {
			
			
			Texture2D state2d = new Texture2D (width,height,TextureFormat.RFloat,false,false);
			state2d.filterMode = FilterMode.Point;
			//state2d.anisoLevel = 0;
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
			
			//debug the positionRT before it's processed by gpu
			DebugRT.Instance.DebugSendToGPUPositionRT(PositionRT);
		}
		
		DebugRT dbgrt;
		
		//this will transfer data in PositionRT into the particle list
		public void SendToCPU_ParticlePosition() {
		
			RenderTexture.active = PositionRT;
			tempPos.ReadPixels(tempRect,0,0,false);
			tempPos.Apply(false);
			RenderTexture.active = null;
			
			//debug position rt after it;s processed by gpu
			//Graphics.Blit(PositionRT,DebugRT.Instance.SendToCPU_PositionRT);
			
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

