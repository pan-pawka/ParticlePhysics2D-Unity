using UnityEngine;
using System.Collections;
using UnityEditor;
using ParticlePhysics2D;

[CustomEditor(typeof(MeshLineRender))]

public class MeshLineRender_Editor :  Editor {
	
	MeshLineRender lr;
	
	void OnEnable() {
		lr = target as MeshLineRender;
		if (!lr.IsInitialized) {
			lr.MeshLineRender_Ctor();
		}
	}
	
	public override void OnInspectorGUI (){
		DrawDefaultInspector();
		EditorGUILayout.Space();
		Color c = GUI.color;
		GUI.backgroundColor = Color.red;
		bool remove = GUILayout.Button(new GUIContent("Remove this renderer and its resources"),GUILayout.ExpandWidth(true));
		if (remove) {
			MeshLineRender lr = target as MeshLineRender;
			DestroyImmediate(lr);
			lr.RemoveResources();
			
		}	
		GUI.backgroundColor = c;
	}
	
}
