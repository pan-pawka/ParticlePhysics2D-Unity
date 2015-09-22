using System;
using UnityEngine;

public static class Verlet_MathUtility  {
	
	static float[] fastSin = new float [361];
	static float[] fastCos = new float [361];
	
	static float deg2Rad = (float)Math.PI / 180f ;
	static float Pi = (float)Math.PI;
	
	static Verlet_MathUtility() {
		
		for (int i = 0; i<=360 ; i++ ) {
			fastSin[i] = (float)Math.Sin( i * deg2Rad );
			fastCos[i] = (float)Math.Cos( i * deg2Rad );
		}
	}
	
	public static float GetFastSin_FloatDeg2Float (float deg){
		
		float absDeg = Math.Abs (deg);
		if (absDeg > 360f) absDeg = absDeg - 360f;
		
		//int signDeg = Math.Sign (deg);
		int minDeg = (int)Math.Floor (absDeg);
		int maxDeg = (int)Math.Ceiling (absDeg);
		float decDeg = absDeg - minDeg ;
		
		if (deg > 0) {
			return ((fastSin[maxDeg] - fastSin[minDeg]) * decDeg + fastSin[minDeg] );
		} else {
			return -((fastSin[maxDeg] - fastSin[minDeg]) * decDeg + fastSin[minDeg] ) ;
		}
		
	}
	
	public static float GetFastCos_FloatDeg2Float ( float deg ) {
		
		float absDeg = Math.Abs (deg);
		if (absDeg > 360f) absDeg = absDeg - 360f;
		int minDeg = (int)Math.Floor (absDeg);
		int maxDeg = (int)Math.Ceiling (absDeg);
		float decDeg = absDeg - minDeg ;
		
		return ((fastCos[maxDeg] - fastCos[minDeg]) * decDeg + fastCos[minDeg] );
	}
	
	//rotate point t around point c for degree c
	public static Vector2 RotateVector2(Vector2 t, Vector2 c, float a){
		
		float cosA = Verlet_MathUtility.GetFastCos_FloatDeg2Float(a);
		float sinA = Verlet_MathUtility.GetFastSin_FloatDeg2Float(a);
		
		return new UnityEngine.Vector2 (
			(cosA * (t.x - c.x) - sinA * (t.y - c.y) + c.x),
			(sinA * (t.x - c.x) + cosA * (t.y - c.y) + c.y)
			);
		
	}
	
	//
	public static Vector2 RotateVector2(Vector2 t,   float a){
		
		float cosA = Verlet_MathUtility.GetFastCos_FloatDeg2Float(a);
		float sinA = Verlet_MathUtility.GetFastSin_FloatDeg2Float(a);
		
		return new UnityEngine.Vector2 (
			(cosA * t.x - sinA * t.y ),
			(sinA * t.x + cosA * t.y )
			);
		
	}
}