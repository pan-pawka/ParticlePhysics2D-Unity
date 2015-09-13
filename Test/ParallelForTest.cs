using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Threading;

public class ParallelForTest : MonoBehaviour {

	public int num = 10000000;
	public static float total;
	public float[] r;
	Stopwatch s;
	float res = 0f;
	
	// Use this for initialization
	void Start () {
		s= new Stopwatch ();
		task = this.ParallelTask;
		r = new float[num];
		total = 0;
		//var s = Interlocked.
		s.Start();
		ParticlePhysics2D.Parallel.For(0,num,task);
		//SeqCalc();
		s.Stop();
		UnityEngine.Debug.Log(s.ElapsedMilliseconds);
		UnityEngine.Debug.Log("Result = "+res);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	System.Action<int> task;
	
	void SeqCalc() {
		for (int i=0;i<num;i++) {
			for (int j=0;j<100;j++) {
				res += Mathf.Sqrt(j);	
			}
		}
	}
	
	void ParallelTask ( int i ) {
		for (int j=0;j<100;j++) {
			//res += Mathf.Sqrt(j);	
			ParticlePhysics2D.Extension.InterlockAddFloat(ref res,Mathf.Sqrt(j));
		}
	}
}
