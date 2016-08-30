//using UnityEngine;
//using System.Collections;
//
//public class GPUTest : MonoBehaviour {
//
//	bool t = false;
//
//	// Use this for initialization
//	void Start () {
//		Application.targetFrameRate = 60;
//		GLNative.Init();
//	}
//	
//	// Update is called once per frame
//	void Update () {
//	
//	}
//	
//	void OnPreRender () {
//		
//		//if (t==false) {
//			//set global line width
//			GLNative.SetGlobalLineWidth(1.5f);
//			//t = true;
//		//}
//		
//		
//	}
//	
//	void OnDestroy() {
//		GLNative.DisableGlobalLineWidth();
//	}
//}
