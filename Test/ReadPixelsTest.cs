using UnityEngine;
using System.Collections;

public class ReadPixelsTest : MonoBehaviour {

	public RenderTexture rt;
	public RenderTexture[] rtt;
	public Texture2D t2d;
	private Shader sd;
	private Material mtl;
	private Camera cam;
	int head = 0;

	// Use this for initialization
	void Start () {
		rt = new RenderTexture (Screen.width,Screen.height,0,RenderTextureFormat.Default);
		rtt = new RenderTexture[5] ;
		for (int i=0;i<5;i++) {
			rtt[i] = new RenderTexture (Screen.width * 2,Screen.height * 2,0,RenderTextureFormat.Default);
			rtt[i].Create();
		}
		
		t2d = new Texture2D (Screen.width * 2,Screen.height * 2,TextureFormat.ARGB32,false,false);
		rt.Create();
		sd = Shader.Find("Sprites/Default");
		Debug.Log(sd);
		mtl = new Material (sd);
		mtl.SetColor("_Color",Color.red);
		cam = Camera.main;
		Camera.main.enabled = false;
		cam.targetTexture = rt;
		
		cam.Render();
		Graphics.Blit(rt,rtt[head],mtl);
	}
	
	// Update is called once per frame
	void Update () {
		
		cam.Render();
		//head++;
		//head %= 5;
		rtt[head].DiscardContents();
		Graphics.Blit(rt,rtt[head],mtl);
		StartCoroutine(ReadPixels());

	}
	
	void OnRenderObject2() {
		//int th = head +1;
		//if (th==5) th = 0;
		int th = 0;
		RenderTexture.active = rtt[th];
		t2d.ReadPixels(new Rect(0, 0, rtt[th].width, rtt[th].height), 0, 0);
		
		rtt[th].DiscardContents();
		t2d.Apply();
		RenderTexture.active = null;//you must set it back to null, otherwise unity crashes.
	}
	
	IEnumerator ReadPixels() {
		yield return new WaitForEndOfFrame();
		OnRenderObject2();
	}
	
	
	
//	RenderTexture.active = myRenderTexture;
//	myTexture2D.ReadPixels(new Rect(0, 0, myRenderTexture.width, myRenderTexture.height), 0, 0);
//	myTexture2D.Apply();
}
