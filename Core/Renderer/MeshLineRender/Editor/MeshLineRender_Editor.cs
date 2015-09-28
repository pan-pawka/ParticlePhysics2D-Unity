using UnityEngine;
using System.Collections;
using UnityEditor;
using ParticlePhysics2D;

[CustomEditor(typeof(MeshLineRender))]

public class MeshLineRender_Editor :  Editor,ISerializationCallbackReceiver {
	
	MeshLineRender lr;
	
	void OnEnable() {
		lr = target as MeshLineRender;
		if (!lr.IsInitialized) {
			lr.MeshLineRender_Ctor();
		}
	}
	
	public void OnBeforeSerialize() {}
	public void OnAfterDeserialize(){
		//Debug.Log("SD");
	}
	
	public override void OnInspectorGUI (){
		//DrawDefaultInspector();
		EditorGUILayout.Space();
		Color cl = lr.color;
		lr.color = EditorGUILayout.ColorField("Line Color",lr.color,GUILayout.ExpandWidth(true));
		if (cl != lr.color) EditorUtility.SetDirty(lr);
		EditorGUILayout.Space();
		
		GUI.backgroundColor = Color.red;
		bool remove = GUILayout.Button(new GUIContent("Remove this renderer and its resources"),GUILayout.ExpandWidth(true));
		if (remove && lr!=null) {
			DestroyImmediate(lr);
			lr.RemoveResources();
			
		}	
		
	}
	
}
