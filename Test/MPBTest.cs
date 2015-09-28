using UnityEngine;
using System.Collections;

public class MPBTest : MonoBehaviour {

	MeshRenderer rd;
	MaterialPropertyBlock mpb;
	RenderTexture rt;
	
	public Texture2D tex1,tex2;
	
	RenderTexture t1,t2;
	
	// Use this for initialization
	void Start () {
		rt = new RenderTexture (300,300,0,RenderTextureFormat.Default);
		t1 = new RenderTexture (300,300,0,RenderTextureFormat.Default);
		t2 = new RenderTexture (300,300,0,RenderTextureFormat.Default);
		
		Graphics.Blit(tex1,t1);
		Graphics.Blit(tex2,t2);
		
		Graphics.Blit(tex1,rt);
		rd = this.GetComponent<MeshRenderer>();
		mpb= new MaterialPropertyBlock ();
		mpb.SetTexture("_MainTex",rt);
		rt = RenderTexture.GetTemporary(300,300,0,RenderTextureFormat.Default);
		Graphics.Blit(t2,rt);
		
		rd.SetPropertyBlock(mpb);
		
			
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
