using UnityEngine;
using System.Collections;

namespace ParticlePhysics2D {
	public static class ObjExtension {
		
		public static void ObjDestroy(this UnityEngine.Object obj) {
			
			if (Application.isEditor) {
				if (obj)  {
					try {
						UnityEngine.Object.DestroyImmediate(obj);	
					} catch{}
				}
			} else {
				if (obj) {
					try {
						UnityEngine.Object.Destroy(obj);	
					} catch{}
				}
			}	
			
		}
	}
}