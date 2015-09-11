using UnityEngine;
using System.Collections;

public class DebugRT : MonoBehaviour {

	public RenderTexture rt;
	
	public RenderTexture rtb;

	// Use this for initialization
	void Start () {
		rtb = new RenderTexture (Screen.width,Screen.height,0,RenderTextureFormat.RGHalf);
	}
	
	// Update is called once per frame
	void Update () {
		if (rt) Graphics.Blit(rt,rtb);
	}
}
