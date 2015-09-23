/// <summary>
/// 我刚才试了下 一个2048 * 2048 的材质，从GPU读取到CPU 每帧耗时 50ms
/// 而 如果把这个2048 * 2048 的材质，划分成 4 * 4 = 16 个小材质的话，分块儿读，一帧耗时 90ms
/// 看来 放在一起读取，还是会快
/// 所以 我可以不把计算batch 在一起，仅仅把读取Batch了
/// </summary>


using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class ReadPixelsTest : MonoBehaviour {

	public RenderTexture rt;
	public RenderTexture[] rtt;
	public Texture2D t2d,t2dt;
	private Shader sd;
	private Material mtl;
	private Camera cam;
	int head = 0;
	
	Stopwatch s;
	
	float time = 60f;

	// Use this for initialization
	void Start () {
	
		s = new Stopwatch ();
		
		rt = new RenderTexture (128,256,0,RenderTextureFormat.Default);
		rt.Create();
		
		rtt = new RenderTexture[16] ;
		for (int i=0;i<16;i++) {
			rtt[i] = new RenderTexture (2048 /4,2048/4,0,RenderTextureFormat.Default);
			rtt[i].Create();
		}
		
		t2d = new Texture2D (2048,2048,TextureFormat.RGBAFloat,false,false);
		t2dt = new Texture2D (2048/4,2048/4,TextureFormat.RGBAFloat,false,false);
		
		sd = Shader.Find("Sprites/Default");
		
		mtl = new Material (sd);
		mtl.SetColor("_Color",Color.red);
		cam = Camera.main;
		Camera.main.enabled = false;
		cam.targetTexture = rt;
		
		cam.Render();
		//Graphics.Blit(rt,rtt[head],mtl);
	}
	
	// Update is called once per frame
	void Update () {
		
		//cam.Render();
		
		StartCoroutine(ReadPixels());

	}
	
	void OnRenderObject2() {
		RenderTexture.active = rt;
		t2d.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
		rt.DiscardContents();
		t2d.Apply();
		RenderTexture.active = null;//you must set it back to null, otherwise unity crashes.
	}
	
	IEnumerator ReadPixels() {
		yield return new WaitForEndOfFrame();
		OnRenderObject2();
	}
	
	void Test () {
		for (int i=0;i<16;i++) {
			RenderTexture.active = rtt[i];
			t2dt.ReadPixels(new Rect(0, 0, rtt[i].width, rtt[i].height), 0, 0);
			rtt[i].DiscardContents();
			t2dt.Apply();
			RenderTexture.active = null;
		}
	}
	
	
	
//	RenderTexture.active = myRenderTexture;
//	myTexture2D.ReadPixels(new Rect(0, 0, myRenderTexture.width, myRenderTexture.height), 0, 0);
//	myTexture2D.Apply();
}
