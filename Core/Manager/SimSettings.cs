using UnityEngine;
using System.Collections;

namespace ParticlePhysics2D {

	[System.Serializable]
	[CreateAssetMenu(fileName = "SimSetting",menuName = "ParticlePhysics2D/SimSetting")]
	public class SimSettings : ScriptableObject {

		public IntegrationMedthod integrationMethod = IntegrationMedthod.Verlet;
		public bool applyString = true;
		public bool applyAngle = true;
		
		[Range(1,10)]
		public int iteration = 1;
		public Vector2 gravity = Vector2.zero;
		
		[Range(0.01f,0.99f)]
		public float damping = 0.9f;
		
		[Range(0.005f,0.99f)]
		public float springConstant = 0.9f;
		
		[Range(0.001f,0.2f)]
		public float angleConstant = 0.2f;
		
		
		public Color springDebugColor = Color.yellow;
		public Color angleDebugColor = Color.cyan;
		
		
		
	}
	
}


