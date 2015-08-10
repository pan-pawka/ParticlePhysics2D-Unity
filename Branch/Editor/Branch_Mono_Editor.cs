using UnityEngine;
using System.Collections;
using UnityEditor;
using ParticlePhysics2D;

[CustomEditor(typeof(Branch_Mono))]
public class Branch_Mono_Editor : Editor {
	
	Simulation sim;
	Branch_Mono temp;
	
	bool reGenBranch;
	
	void OnEnable() {
		temp = target as Branch_Mono;
		sim = temp.sim;
		Branch.lengthExit = temp.lengthExit;
		Branch.angleOffsetMax = temp.angleOffsetMax;
		Branch.angleOffsetMin = temp.angleOffsetMin;
		Branch.lengthMax1 = temp.lengthMax1;
		Branch.lengthMax2 = temp.lengthMax2;
		Branch.lengthMin1 = temp.lengthMin1;
		Branch.lengthMin2 = temp.lengthMin2;
		Branch.lengthBranchAThreshold = temp.lengthBranchAThreshold;
		Branch.lengthBranchBThreshold = temp.lengthBranchBThreshold;
	}
	
	void OnDisable() {
		temp.lengthExit = Branch.lengthExit;
		temp.angleOffsetMax = Branch.angleOffsetMax;
		temp.angleOffsetMin = Branch.angleOffsetMin;
		temp.lengthMax1 = Branch.lengthMax1;
		temp.lengthMax2 = Branch.lengthMax2;
		temp.lengthMin1 = Branch.lengthMin1;
		temp.lengthMin2 = Branch.lengthMin2;
		temp.lengthBranchAThreshold = Branch.lengthBranchAThreshold;
		temp.lengthBranchBThreshold = Branch.lengthBranchBThreshold;
	
	}

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();
		
		//Branch params
		EditorGUILayout.LabelField("-----------------------------------------------------");
		//exit length: how complex should the branch be
		Branch.lengthExit = EditorGUILayout.Slider(
			new GUIContent("Length Exit","This generally controls how complex the branch is"),
			Branch.lengthExit,
			temp.length *0.01f,
			temp.length 
		);
		
		//how wide
		EditorGUILayout.MinMaxSlider(
			new GUIContent("Angle Offset","How much should the children branch be rotated from parent" ),
			ref Branch.angleOffsetMin,
			ref Branch.angleOffsetMax,
			0f,
			1f
		);
		
		//branch threshold A
		Branch.lengthBranchAThreshold = EditorGUILayout.Slider(
			new GUIContent("Threshold A",""),
			Branch.lengthBranchAThreshold,
			0.01f,
			temp.length
		);
		
		//branch threshold A
		Branch.lengthBranchBThreshold = EditorGUILayout.Slider(
			new GUIContent("Threshold B",""),
			Branch.lengthBranchBThreshold,
			0.01f,
			temp.length
		);
		
		//above threshold
		EditorGUILayout.MinMaxSlider(
			new GUIContent("L. Factor Above Threshold","When children branch go over threshold, the factor multiplied to parent branch" ),
			ref Branch.lengthMin1,
			ref Branch.lengthMax1,
			0f,
			1f
		);
		
		//above threshold
		EditorGUILayout.MinMaxSlider(
			new GUIContent("L. Factor Below Threshold","When children branch go below threshold, the factor multiplied to parent branch" ),
			ref Branch.lengthMin2,
			ref Branch.lengthMax2,
			0f,
			1f
		);
			
		reGenBranch = GUILayout.Button("Re-Generate Branch",GUILayout.ExpandWidth(true));
		if (reGenBranch) {
			temp.ReGenerateBranch();
			EditorUtility.SetDirty(temp);
		}
		
		bool resetParam = false;
		resetParam = GUILayout.Button("Reset Branch Generation Params",GUILayout.ExpandWidth(true));
		if (resetParam) Branch.ResetParams();
		
		//clear branch
		bool clearBranch = GUILayout.Button("Clear Branchs",GUILayout.ExpandWidth(true));
		if (clearBranch) {
			sim.clear();
			sim.clearForces();
			temp.branch = null;
			EditorUtility.SetDirty(temp);
		}
		
		EditorGUILayout.HelpBox("Particle Count: "+sim.numberOfParticles(),MessageType.None);
		
	}
	
	public void OnSceneGUI() {
		if (sim!=null) {
			if (temp.debugIndex)
				//sim.DebugParticle(temp.transform.localToWorldMatrix);
				sim.DebugParticle();
		}
	}
}
