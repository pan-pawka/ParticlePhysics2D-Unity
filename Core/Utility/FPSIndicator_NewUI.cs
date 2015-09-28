using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPSIndicator_NewUI : MonoBehaviour {

	float updateInterval = 0.5f;
	float accum;
	float frames;
	float timeleft;
	
	public Text text;//drap and drop fr editor 

	void Start () {
		
		if (!text) {
			print ("FramesPerSecond needs a 2dtk text mesh component!");
			this.enabled = false;
			return;
		} else {
			//text.MakePixelPerfect();
			//text.anchor = TextAnchor.UpperLeft;
			
		}
		timeleft = updateInterval; 
	
	}
	
	// Update is called once per frame
	void Update () {
	
		timeleft -= Time.deltaTime;
		accum += Time.timeScale/Time.deltaTime;
		++frames;
		
		// Interval ended - update GUI text and start new interval
		if( timeleft <= 0.0f )
		{
			// display two fractional digits (f2 format)
			text.text = "FPS : " + (accum/frames).ToString("f2");
			timeleft = updateInterval;
			accum = 0.0f;
			frames = 0f;
			//text.Commit();
		}
	
	}
}
