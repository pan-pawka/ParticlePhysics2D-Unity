using UnityEngine;
using System.Collections;

public class QuadMeshTest : MonoBehaviour {

	public Mesh quadMesh;
	
	public Material quadMeshMtl;
	
	public Texture2D testImg;
	
	MaterialPropertyBlock mpb;
	
	public RenderTexture rt;
	


	// Use this for initialization
	void Start () {
	
		rt = new RenderTexture (Screen.width,Screen.height,0,RenderTextureFormat.ARGBFloat);
		
		Camera.main.targetTexture = rt;
		//Camera.main.hdr = true;
	
		mpb = new MaterialPropertyBlock ();
		mpb.SetTexture("_MainTex",testImg);
		
		//quadMeshMtl.SetTexture("_MainTex",testImg);
		GenerateQuadMesh();
		
		UnityEngine.Rendering.CommandBuffer cbuffer;
		cbuffer = new UnityEngine.Rendering.CommandBuffer ();
		cbuffer.SetRenderTarget(rt);
		cbuffer.DrawMesh(quadMesh,Matrix4x4.identity,quadMeshMtl,0,-1,mpb);
		Graphics.ExecuteCommandBuffer(cbuffer);
		//Graphics.DrawMesh(quadMesh,Matrix4x4.identity,quadMeshMtl,LayerMask.NameToLayer("Default"),Camera.main,0,mpb,false,false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
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
}
