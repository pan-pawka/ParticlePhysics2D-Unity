//Yves Wang @ FISH, 2015, All rights reserved
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ParticlePhysics2D {
	public static class Extension {
		
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
		
		//shuffle a list
		public static void Shuffle<T>(this IList<T> list)  
		{  
			int n = list.Count;  
			while (n > 1) {  
				n--;  
				int k = Random.Range(0,n+1);  
				T value = list[k];  
				list[k] = list[n];  
				list[n] = value;  
			}  
		}
		
		
		
	}
	
	//threading
	public static class Parallel {
		public static void For(int fromInclusive, int toExclusive, System.Action<int> body) {
			var numThreads = 2 * System.Environment.ProcessorCount;
			var resets = new ManualResetEvent[numThreads];
			for (var i = 0; i < numThreads; i++) {
				var reset = new ManualResetEvent(false);
				resets[i] = reset;
				ThreadPool.QueueUserWorkItem((j) => {
					for (var k = (int)j; k < toExclusive; k += numThreads)
						body((int)k);
					reset.Set();
				}, fromInclusive + i);
			}
			
			for (var i = 0; i < numThreads; i++)
				resets [i].WaitOne ();
		}
	}
}