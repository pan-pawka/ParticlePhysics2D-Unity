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
		
		//add float value to location1, as a atomic action
		//http://stackoverflow.com/questions/1400465/why-is-there-no-overload-of-interlocked-add-that-accepts-doubles-as-parameters
		public static double InterlockAddFloat(ref float location1, float value)
		{
			float newCurrentValue = 0;
			while (true)
			{
				float currentValue = newCurrentValue;
				float newValue = currentValue + value;
				newCurrentValue = Interlocked.CompareExchange(ref location1, newValue, currentValue);
				if (newCurrentValue == currentValue)
					return newValue;
			}
		}
		
		
		
	}
	
	//threading
	public static class Parallel {
	
		static int numThreads;
		static ManualResetEvent[] resets;
		
		static Parallel() {
			numThreads  = 2 * System.Environment.ProcessorCount;
			resets = new ManualResetEvent[numThreads];
			for (int i=0;i<numThreads;i++) {
				resets[i] = new ManualResetEvent (false);
			}
			Debug.Log(numThreads);
		}
		
		public static void WaitAll() {
			for (var i = 0; i < numThreads; i++) resets [i].WaitOne (1000);
		}
		
		public static void For(int fromInclusive, int toExclusive, System.Action<int> body, bool isWaitAll = true) {
			
			for (var i = 0; i < numThreads; i++) {
				ManualResetEvent reset = resets[i];
				reset.Reset();
				ThreadPool.QueueUserWorkItem(
					(j) => {
						for (var k = (int)j; k < toExclusive; k += numThreads) body((int)k);
						reset.Set();
					}, 
					fromInclusive + i
				);
			}
			if (isWaitAll) WaitAll();
		}
		
		
	}
}