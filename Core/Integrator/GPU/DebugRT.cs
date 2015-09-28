using UnityEngine;
using System.Collections;

public class DebugRT : Singleton<DebugRT> {

	//singleton instance
	public static DebugRT Instance {
		get { return ((DebugRT)mInstance);} 
		set { mInstance = value;}
	}
	
	public RenderTexture SendToGPU_PositionRT , SendToCPU_PositionRT;
	public RenderTexture springDeltaRT,springRT,restLength2RT;
	
	public RenderTexture spDeltaPos;
	
	public RenderTexture angleDelta;
	
	public Mesh deltamesh;
	
	public Material debugMtl;

	// Use this for initialization
	void Start () {
		SendToGPU_PositionRT = new RenderTexture (Screen.width,Screen.height,0,RenderTextureFormat.RGFloat);
		SendToGPU_PositionRT.Create();
		
		SendToCPU_PositionRT = new RenderTexture (Screen.width,Screen.height,0,RenderTextureFormat.RGFloat);
		SendToCPU_PositionRT.Create();
		
		springDeltaRT = new RenderTexture (Screen.width,Screen.height,0,RenderTextureFormat.RGFloat);
		springDeltaRT.filterMode = FilterMode.Point;
		springDeltaRT.Create();
		
		springRT = new RenderTexture (Screen.width,Screen.height,0,RenderTextureFormat.ARGBFloat);
		springRT.filterMode = FilterMode.Point;
		springRT.Create();
		
		restLength2RT = new RenderTexture (Screen.width,Screen.height,0,RenderTextureFormat.ARGBFloat);
		restLength2RT.filterMode = FilterMode.Point;
		restLength2RT.Create();
		
		spDeltaPos = new RenderTexture (Screen.width,Screen.height,0,RenderTextureFormat.RGFloat);
		spDeltaPos.filterMode = FilterMode.Point;
		spDeltaPos.Create();
		
		angleDelta = new RenderTexture (Screen.width,Screen.height,0,RenderTextureFormat.RFloat);
		angleDelta.filterMode = FilterMode.Point;
		angleDelta.Create();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void DebugSendToGPUPositionRT(RenderTexture rt) {
		Graphics.Blit(rt,SendToGPU_PositionRT,debugMtl);
	}
	
	public void DebugSendToCPUPositionRT(RenderTexture rt) {
		Graphics.Blit(rt,SendToCPU_PositionRT,debugMtl);
	}
	
	public void DebugSpringDelta(RenderTexture rt) {
		Graphics.Blit(rt,springDeltaRT,debugMtl);
	}
	
	public void DebugSpringRT(Texture rt) {
		Graphics.Blit(rt,springRT,debugMtl);
	}
	
	public void DebugRestLengthRT (Texture rt) {
		Graphics.Blit(rt,restLength2RT);
	}
}
