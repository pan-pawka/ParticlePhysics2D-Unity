using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace ParticlePhysics2D {

	public class SimBuffer_Angle  {
		
		
		RenderTexture[] tempRT;//store the temp position rt, 
		RenderTexture[] tempDeltaRT;//store the temp delta
		RenderTexture[] paramRT;//rgba float, rg= the other end'uv in positionRT, ba = uv in the deltaRT
		
		int deltaRTWidth,deltaRTHeight;
		Vector2[] angleUV;
		
		int ID_AngleParamRT,ID_AngleConstant,ID_PositionRT,ID_AngleDeltaRT;
		Simulation sim;
		MaterialPropertyBlock[] mpb;
		
		Mesh[] deltaMesh;
		
		
		
		public SimBuffer_Angle (Simulation sim,Vector2[] particleUV,int width,int height) {
		
			this.sim = sim;
			int angleNum = sim.numberOfAngleConstraints();
			int parNum = sim.numberOfParticles();
			
			//angle uv
			float usage = 0f;
			if (!SimBuffer.GetTexDimension(angleNum,out deltaRTWidth,out deltaRTHeight, out usage)) {
				Debug.LogError("Cannot create SimBuffer Angle deltaw rt with wrong dimesnion!");
				return;
			}
			angleUV = new Vector2[angleNum];
			int count = 0;
			float halfW = 0.5f/deltaRTWidth;
			float halfH = 0.5f/deltaRTHeight;
			for (int y=0;y<deltaRTHeight;y++) {
				for (int x=0;x<deltaRTWidth;x++) {
					if (count<angleNum) {
						angleUV[count] = new Vector2 ((float)x/(float)deltaRTWidth + halfW , (float)y/(float)deltaRTHeight + halfH);
					}
					count++;
				}
			}
			
			tempRT = new RenderTexture[sim.maxAngleConvergenceID-1];
			tempDeltaRT = new RenderTexture[sim.maxAngleConvergenceID];
			paramRT = new RenderTexture[sim.maxAngleConvergenceID];
			deltaMesh = new Mesh[sim.maxAngleConvergenceID];
			
			ID_AngleParamRT = Shader.PropertyToID("_AngleParamRT");
			ID_AngleConstant = Shader.PropertyToID("_AngleConstant");
			ID_PositionRT = Shader.PropertyToID("_PositionRT");
			ID_AngleDeltaRT = Shader.PropertyToID("_AngleDeltaRT");
			
			//rg = the other end point's uv, ba = uv in the delta rt
			Color[] paramRTColor = new Color[width * height];
			Texture2D tempTex = new Texture2D (width,height,TextureFormat.RGBAFloat,false,false);
			
			for (int i=0;i<sim.maxAngleConvergenceID;i++) {
				
				//delta mesh
				int agbyid = sim.numberOfAnglesByConvID(i+1);
				deltaMesh[i] = SimBuffer.PointMesh(agbyid);
				List<Vector3> vtc = new List<Vector3> (agbyid);//xy = a uv,y=fixedangle
				List<Color> cl = new List<Color> (agbyid);//rg=b uv,ba = m uv;
				List<Vector2> uv = new List<Vector2> (agbyid);//delta rt uv;
				
				//init rt
				paramRT[i] = new RenderTexture (width,height,0,RTFormat.ARGB);
				paramRT[i].Create();
				
				//prepare temp color
				for (int k=0;k<width * height;k++) {
					if (k<parNum) {
						Vector2 puv = particleUV[k];
						paramRTColor[k] = new Color (puv.x,puv.y,0f,0f);//rg = other end,ba = uv in deltart
					} else {
						paramRTColor[k] = Color.clear;
					}
				}
				
				//get info
				for (int j=0;j<angleNum;j++) {
					AngleConstraint2D ag = sim.getAngleConstraint(j);
					if (ag.convergenceGroupID == i+1) {
						int a = sim.getParticleIndex(ag.ParticleB);
						int b = sim.getParticleIndex(ag.ParticleM);
						int aIndex = sim.getParticleIndex(ag.ParticleA);
						//if it's free, rg = the other end's uv, else = own uv
						if (ag.ParticleB.IsFree) {
							paramRTColor[a].r = particleUV[b].x;
							paramRTColor[a].g = particleUV[b].y;
						}
						if (ag.ParticleM.IsFree) {
							paramRTColor[b].r = particleUV[a].x;
							paramRTColor[b].g = particleUV[a].y;
						}
						paramRTColor[a].b = paramRTColor[b].b = angleUV[j].x;
						paramRTColor[a].a = paramRTColor[b].a = angleUV[j].y;
						
						//mesh vtc and cl
						vtc.Add(new Vector3 (angleUV[j].x,angleUV[j].y,ag.angle_Fixed));
						uv.Add(particleUV[aIndex]);
						cl.Add(new Color (particleUV[a].x,particleUV[a].y,particleUV[b].x,particleUV[b].y));
						
					}
				}
				
				//blit
				tempTex.SetPixels(paramRTColor);
				tempTex.Apply();
				Graphics.Blit(tempTex,paramRT[i]);
				
				//delta mesh
				deltaMesh[i].vertices = vtc.ToArray();
				deltaMesh[i].colors = cl.ToArray();
				deltaMesh[i].uv = uv.ToArray();
				//deltaMesh[i].UploadMeshData(true);
			}
			
			Extension.ObjDestroy(tempTex);
			
			//mpb
			mpb = new MaterialPropertyBlock[sim.maxSpringConvergenceID];
			for (int i=0;i<sim.maxAngleConvergenceID;i++) {
				mpb[i] = new MaterialPropertyBlock ();
				mpb[i].SetTexture(ID_AngleParamRT,paramRT[i]);
				mpb[i].SetFloat(ID_AngleConstant,sim.Settings.angleConstant);
			}
			
		}
		
		//called from SimBuffer
		public void Blit(ref CommandBuffer cBuffer, ref RenderTexture sourcePosRT, ref RenderTexture destPosRT) {
			
			//get temp RT
			for (int i=0;i<tempRT.Length;i++) tempRT[i] = SimBuffer.GetTempRT(destPosRT);
			for (int i=0;i<tempDeltaRT.Length;i++) tempDeltaRT[i] = SimBuffer.GetTempRT(deltaRTWidth,deltaRTHeight,RTFormat.R);
			
			//init mpb positionRT;
			for (int i=0;i<sim.maxAngleConvergenceID;i++) {
				if (i==0) mpb[i].SetTexture(ID_PositionRT,sourcePosRT);
				else mpb[i].SetTexture(ID_PositionRT,tempRT[i-1]);
				mpb[i].SetTexture(ID_AngleDeltaRT,tempDeltaRT[i]);
			}
			
			for (int i=0;i<sim.maxAngleConvergenceID;i++) {
				//get the delta
				cBuffer.SetRenderTarget(tempDeltaRT[i]);
				cBuffer.DrawMesh(deltaMesh[i],Matrix4x4.identity,GPUVerletIntegrator.angleDeltaMtl,0,-1,mpb[i]);
				//apply delta
				if (i==sim.maxAngleConvergenceID-1) cBuffer.SetRenderTarget(destPosRT);//if it is the last step, blit into the final dest rt
				else cBuffer.SetRenderTarget(tempRT[i]);
				cBuffer.DrawMesh(SimBuffer.quadMesh,Matrix4x4.identity,GPUVerletIntegrator.angleMtl,0,-1,mpb[i]);
			}
			
			
		}
		
		public void ReleaseTempRT() {
			//release temp rt
			for (int i=0;i<tempRT.Length;i++) RenderTexture.ReleaseTemporary(tempRT[i]);
			for (int i=0;i<tempDeltaRT.Length;i++) RenderTexture.ReleaseTemporary(tempDeltaRT[i]);
		}
		
	}
	
}
