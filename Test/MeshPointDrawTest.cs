using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;

public class MeshPointDrawTest : MonoBehaviour {
	
	public Mesh mesh;
	public int numberofpoints = 10;
	
	public RenderTexture rt,rt2;
	
	Material mtl;
	
	public Mesh someMesh;
	
	public Transform testMesh;
	
	// Use this for initialization
	void Start () {
		mesh = PointMesh(numberofpoints);
		mtl = new Material (Shader.Find("ParticlePhysics2D/SpringConstraint"));
		rt = new RenderTexture (Screen.width/20,Screen.height/20,0,RenderTextureFormat.ARGBFloat);
		rt2 = new RenderTexture (Screen.width,Screen.height,0,RenderTextureFormat.ARGBFloat);
		rt.filterMode = FilterMode.Point;
		rt.Create();
		
		Debug.Log("Test Mesh L2W matrix : " + testMesh.transform.localToWorldMatrix);
		Debug.Log("Camera View matrix : " + Camera.main.transform.worldToLocalMatrix);
		Debug.Log("Camera proj matrix : " + Camera.main.projectionMatrix);
		Debug.Log ("GPU Projection Matrix: "+GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix,true));
		
		//rt2.DiscardContents();
		//Graphics.Blit(Texture2D.blackTexture,rt2);
		Graphics.SetRenderTarget(rt2);
		mtl.SetPass(0);
		Matrix4x4 trs= Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one * 100f);
		Graphics.DrawMeshNow(mesh,Matrix4x4.identity);
		//Graphics.Blit(rt,rt2);
	}
	
	Mesh PointMesh ( int n) {
		Mesh o = new Mesh ();
		//Vector3[] vert = new Vector3[n];
		Vector3[] vert = Enumerable.Repeat<Vector3>(Vector3.one ,n).ToArray();
		//for (int i=0;i<n;i++) vert[i] = new Vector3 (Random.value,Random.value,Random.value);
		o.vertices = vert;
		int[] ind = new int[n];
		for (int i=0;i<n;i++) {
			ind[i] = i;
		}
		//o.normals = Enumerable.Repeat<Vector3>(Vector3.forward,n).ToArray();
		o.SetIndices(ind,MeshTopology.Points,0);
		o.RecalculateBounds();
		o.Optimize();
		o.UploadMeshData(false);
		
		AssetDatabase.CreateAsset(o, "Assets/PointMesh.asset");
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
		return o;
	}
	
}
