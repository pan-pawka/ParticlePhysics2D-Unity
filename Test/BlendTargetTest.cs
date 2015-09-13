using UnityEngine;
using System.Collections;

public class BlendTargetTest : MonoBehaviour {

	int width,height;
	public RenderTexture rt;
	public Shader sd;
	Material mtl;
	
	
	void Start () {
		mtl = new Material (sd);
		width = Screen.width;
		height = Screen.height;
		Texture2D tex = new Texture2D (width,height,TextureFormat.RGB24,false,true);
		Color[] c = new Color[width * height];
		for (int i = 0;i< width * height ;i++) {
			c[i] = Color.red;
		}
		tex.SetPixels(c);
		tex.Apply(false);
		rt = new RenderTexture (width,height,0,RenderTextureFormat.Default);
		Graphics.Blit(tex,rt);
		Graphics.SetRenderTarget(rt);
		Graphics.Blit(tex,rt,mtl);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
