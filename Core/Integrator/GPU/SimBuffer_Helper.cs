using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace ParticlePhysics2D {
	
	public partial class SimBuffer {
	
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		/// Init helpers
		//////////////////////////////////////////////////////////////////////////////////////////////////////
	
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
		
		public static Mesh PointMesh ( int n) {
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
						counter++;
					} 
				}
			}
		}
		
//		void GenerateSpringMesh () {
//			springMesh = new Mesh[sim.maxSpringConvergenceID];
//			//vertex.xy = final output uv in PositionRT, vertex.z = restlength
//			//Color.rgba = a uv and b uv in PositionRT
//			for (int i=0;i<sim.maxSpringConvergenceID;i++) {
//				int springCount = sim.numberOfSpringsByConvID(i+1);
//				springMesh[i] = PointMesh(springCount);
//				List<Vector3> vtc = new List<Vector3> (springCount * 2);
//				List<Color> cl = new List<Color> (springCount * 2);
//				for (int j=0;j<sim.numberOfSprings();j++) {
//					Spring2D sp = sim.getSpring(j);
//					if (sp.convergenceGroupID == i+1) {
//						int a = sim.getParticleIndex(sp.ParticleA);
//						int b = sim.getParticleIndex(sp.ParticleB);
//						vtc.Add(new Vector3 (particleUV[a].x,particleUV[a].y,sp.restLength2));
//						cl.Add(new Color (particleUV[a].x,particleUV[a].y,particleUV[b].x,particleUV[b].y));
//						vtc.Add(new Vector3 (particleUV[b].x,particleUV[b].y,sp.restLength2));
//						cl.Add(new Color (particleUV[a].x,particleUV[a].y,particleUV[b].x,particleUV[b].y));
//					}
//				}
//				springMesh[i].vertices = vtc.ToArray();
//				springMesh[i].colors = cl.ToArray();
//				springMesh[i].UploadMeshData(true);
//			}
//		}
//		
//		void GenerateAngleMesh () {
//			angleMesh = new Mesh[sim.maxSpringConvergenceID];
//			
//			//vertex.xy = final output uv in PositionRT, vertex.z = fixed angle
//			//color.rgba = a and b uv in positionRT
//			for (int i=0;i<sim.maxSpringConvergenceID;i++) {
//				int angleCount = sim.numberOfAnglesByConvID(i+1);
//				angleMesh[i] = PointMesh(angleCount);
//				List<Vector3> vtc = new List<Vector3> (angleCount * 2);
//				List<Color> cl = new List<Color> (angleCount * 2);
//				for (int j=0;j<sim.numberOfAngleConstraints();j++) {
//					AngleConstraint2D ag = sim.getAngleConstraint(j);
//					if (ag.convergenceGroupID == i+1) {
//						int a = sim.getParticleIndex(ag.ParticleB);
//						int b = sim.getParticleIndex(ag.ParticleM);
//						vtc.Add(new Vector3 (particleUV[a].x,particleUV[a].y,ag.angle_Fixed));
//						cl.Add(new Color (particleUV[a].x,particleUV[a].y,particleUV[b].x,particleUV[b].y));
//						vtc.Add(new Vector3 (particleUV[b].x,particleUV[b].y,ag.angle_Fixed));
//						cl.Add(new Color (particleUV[a].x,particleUV[a].y,particleUV[b].x,particleUV[b].y));
//					}
//				}
//				angleMesh[i].vertices = vtc.ToArray();
//				angleMesh[i].colors = cl.ToArray();
//				angleMesh[i].UploadMeshData(true);
//			}
//		}
//		
//		void GenerateAngleRT() {
//
//		}
		
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
		
		#region Data Transfer
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
			//Graphics.Blit(state2d,StateRT);
			Extension.ObjDestroy(state2d);
		}
		
		/// <summary>
		/// this will get all the particle position into PositionRT, usually after collision response
		/// </summary>
		Color[] particlePositionColor;
		public void SendToGPU_ParticlePosition() {
			int counter = 0;
			for (int i=0;i<height;i++) {
				for (int j=0;j<width;j++) {
					Vector2 pPos;
					if (counter<sim.numberOfParticles()) {
						pPos = sim.getParticle(counter).Position;
					} else {
						pPos = Vector2.zero;
					}
					particlePositionColor[counter].r = pPos.x;
					particlePositionColor[counter].g = pPos.y;
					counter++;
				}
			}
			
			tempPos.SetPixels(particlePositionColor);
			tempPos.Apply(false);
			Graphics.Blit(tempPos,PositionRT);
		}
		
		DebugRT dbgrt;
		
		//this will transfer data in PositionRT into the particle list
		public void SendToCPU_ParticlePosition() {
			
			RenderTexture.active = PositionRT;
			tempPos.ReadPixels(tempRect,0,0,false);
			tempPos.Apply(false);
			RenderTexture.active = null;
			
			PositionRT.DiscardContents();
			particlePositionColor = tempPos.GetPixels();
			int counter = 0;
			for (int i=0;i<height;i++) {
				for (int j=0;j<width;j++) {
					if (counter<sim.numberOfParticles()) {
						Vector2 pos = new Vector2 (particlePositionColor[counter].r,particlePositionColor[counter].g);
						sim.getParticle(counter).Position = pos;
					}
					counter++;
				}
			}
			
		}
		#endregion
		
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
		
		public static RenderTexture GetTempRT(int w,int h,RenderTextureFormat format) {
			RenderTexture nrt = RenderTexture.GetTemporary(w,h,0,format);
			nrt.filterMode = FilterMode.Point;
			return nrt;
		}
		
		public static RenderTexture GetTempRT(RenderTexture rt) {
			return GetTempRT(rt.width,rt.height,rt.format);
		}
		
	}
}