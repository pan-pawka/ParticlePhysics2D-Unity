using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

namespace ParticlePhysics2D {

	public class SimBuffer_Spring  {
	
		RenderTexture[] rt;//params rt
		int ID_SpringParamRT,ID_SpringConstant,ID_PositionRT;
		Simulation sim;
		MaterialPropertyBlock[] mpb;
		
		RenderTexture[] tempRT;
		
		
		public SimBuffer_Spring (Simulation sim,Vector2[] particleUV,int width,int height) {
			
			this.sim = sim;
			rt = new RenderTexture[sim.maxSpringConvergenceID];
			int parNum = sim.numberOfParticles();
			ID_SpringParamRT = Shader.PropertyToID("_SpringParamRT");
			ID_SpringConstant = Shader.PropertyToID("_SpringConstant");
			ID_PositionRT = Shader.PropertyToID("_PositionRT");
			
			//rg = the other end point's uv, b = restlength, a = state
			Color[] tempColor = new Color[width * height];
			Texture2D tempTex = new Texture2D (width,height,TextureFormat.RGBAFloat,false,false);
			
			for (int i=0;i<sim.maxSpringConvergenceID;i++) {
			
				//init rt
				rt[i] = new RenderTexture (width,height,0,RTFormat.ARGB);
				rt[i].Create();
				
				//prepare temp color
				for (int k=0;k<width * height;k++) {
					if (k<parNum) {
						Vector2 uv = particleUV[k];
						tempColor[k] = new Color (uv.x,uv.y,1f,(sim.getParticle(k).IsFree) ? 1f : 0f);
					} else {
						tempColor[k] = new Color (0f,0f,1f,0f);
					}
				}
				
				//get info
				for (int j=0;j<sim.numberOfSprings();j++) {
					Spring2D sp = sim.getSpring(j);
					if (sp.convergenceGroupID == i+1) {
						int a = sim.getParticleIndex(sp.ParticleA);
						int b = sim.getParticleIndex(sp.ParticleB);
						tempColor[a].r = particleUV[b].x;
						tempColor[a].g = particleUV[b].y;
						tempColor[a].b = sp.restLength2;
						tempColor[a].a = (sp.ParticleA.IsFree) ? 1f : 0f;
						tempColor[b].r = particleUV[a].x;
						tempColor[b].g = particleUV[a].y;
						tempColor[b].b = sp.restLength2;
						tempColor[b].a = (sp.ParticleB.IsFree) ? 1f : 0f;
					}
				}
				
				//blit
				tempTex.SetPixels(tempColor);
				tempTex.Apply();
				Graphics.Blit(tempTex,rt[i]);
				
			}
			
			Extension.ObjDestroy(tempTex);
			
			//mpb
			mpb = new MaterialPropertyBlock[sim.maxSpringConvergenceID];
			for (int i=0;i<sim.maxSpringConvergenceID;i++) {
				mpb[i] = new MaterialPropertyBlock ();
				mpb[i].SetTexture(ID_SpringParamRT,rt[i]);
				mpb[i].SetFloat(ID_SpringConstant,sim.Settings.springConstant);
			}
			
			//tempRT
			tempRT = new RenderTexture[sim.maxSpringConvergenceID-1];
		}
		
		//called from SimBuffer
		public void Blit(ref CommandBuffer cBuffer, ref RenderTexture sourcePosRT, ref RenderTexture destPosRT) {
		
			//get temp RT
			for (int i=0;i<tempRT.Length;i++) 
				tempRT[i] = SimBuffer.GetTempRT(destPosRT);
			
			//init mpb positionRT;
			for (int i=0;i<sim.maxSpringConvergenceID;i++) {
				if (i==0) mpb[i].SetTexture(ID_PositionRT,sourcePosRT);
				else mpb[i].SetTexture(ID_PositionRT,tempRT[i-1]);
			}
			
			for (int i=0;i<sim.maxSpringConvergenceID;i++) {
				if (i==sim.maxSpringConvergenceID-1) cBuffer.SetRenderTarget(destPosRT);//if it is the last step, blit into the final dest rt
				else cBuffer.SetRenderTarget(tempRT[i]);
				cBuffer.DrawMesh(SimBuffer.quadMesh,Matrix4x4.identity,GPUVerletIntegrator.springMtl,0,-1,mpb[i]);
			}
		}
		
		public void ReleaseTempRT() {
			for (int i=0;i<tempRT.Length;i++) RenderTexture.ReleaseTemporary(tempRT[i]);
		}
		
	}
	
}
