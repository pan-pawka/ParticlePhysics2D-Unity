//Copyright(c),2015 - Yves Wang @ FISH - All rights reserved
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GaussBlur05 : MonoBehaviour {

	Material mat;
	
	void Start () {
		mat = new Material (Shader.Find("FISH/GaussBlur05"));
		mat.SetFloat("_ScreenDeltaU",1f/Screen.width);
		mat.SetFloat("_ScreenDeltaV",1f/Screen.height);
	}
	
	//[ImageEffectTransformsToLDR]
	void OnRenderImage(RenderTexture src, RenderTexture dest) {
		//Debug.Log(src.format.ToString()); it seems that unity uses ARGBHalf for HDR rendering, why not ARGBFloat ?
		
		Graphics.Blit(src, dest, mat);
	}
}
