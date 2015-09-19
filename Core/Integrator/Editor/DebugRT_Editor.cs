using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(DebugRT))]
public class DebugRT_Editor : Editor {

	DebugRT debugrt;

	void OnEnable() {
		debugrt = target as DebugRT;
	}
	
	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();
		
//		debugrt.springDeltaRT = (RenderTexture) EditorGUILayout.ObjectField(
//			"Spring Delta RT",
//			debugrt.springDeltaRT,
//			typeof(RenderTexture),
//			false,
//			GUILayout.ExpandWidth(true),
//			GUILayout.MinHeight(EditorGUIUtility.currentViewWidth/1.5f )
//			);
	}
}
