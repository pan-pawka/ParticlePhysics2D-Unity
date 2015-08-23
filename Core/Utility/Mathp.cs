using System;
using UnityEngine;

namespace ParticlePhysics2D {
	public static class Mathp  {
		
		const int maxIntervals = 10000;
		
		static float[] fastSin = new float [maxIntervals];
		static float[] fastCos = new float [maxIntervals];
		
		//static float deg2Rad = (float)Math.PI / 180f ;
		static float Pi = (float)Math.PI;
		
		static Mathp() {
			float interval = Pi * 2f / maxIntervals;
			float rad = 0f;
			for (int i = 0; i<maxIntervals ; i++ ) {
				fastSin[i] = (float)Math.Sin( rad );
				fastCos[i] = (float)Math.Cos( rad );
				rad += interval;
			}
		}
		static float ValidateAngle(float angleDegrees) {
			if (angleDegrees<360f && angleDegrees >=0) return angleDegrees;
			else {
				if (angleDegrees>=0) {
					int i = (int)Math.Floor(angleDegrees / 360f);
					return angleDegrees - i * 360f;
				} else {
					int i = -(int)Math.Ceiling(angleDegrees / 360f) + 1;
					return angleDegrees + i * 360f;
				}
			}
			
		}
		static int indexClamp(int index) {
			if (index>=maxIntervals) return maxIntervals-1;
			else if (index < 0) return 0;
			else return index;
		}
		static int Index (float angleDegree) {
			return (int)Math.Floor(ValidateAngle(angleDegree) / 360f * (float)maxIntervals);
		}
		
		public static float FastSin (float deg){
			return fastSin[indexClamp(Index(deg))];
		}
		
		public static float FastCos ( float deg ) {
			return fastCos[indexClamp(Index(deg))];
		}
		
		//rotate point t around point c for degree a
		public static Vector2 RotateVector2(Vector2 t, Vector2 c, float a, bool useLUT = false){
			float cosA,sinA;
			if (useLUT) {
				cosA = FastCos(a);
				sinA = FastSin(a);
			} else {
				cosA = Mathf.Cos(a);
				sinA = Mathf.Sin(a);
			}
			
			return new UnityEngine.Vector2 (
				(cosA * (t.x - c.x) - sinA * (t.y - c.y) + c.x),
				(sinA * (t.x - c.x) + cosA * (t.y - c.y) + c.y)
			);
			
		}
		
		//
		public static Vector2 RotateVector2(Vector2 t,   float a){
			
			float cosA = FastCos(a);
			float sinA = FastSin(a);
			
			return new UnityEngine.Vector2 (
				(cosA * t.x - sinA * t.y ),
				(sinA * t.x + cosA * t.y )
			);
			
		}
		
//		public static Vector2 RotateVector2_BuiltIn (Vector2 t, Vector2 c, float a){ 
//			float cosA = Mathf.Cos(a);
//			float sinA = Mathf.Sin(a);
//			
//			return new UnityEngine.Vector2 (
//				(cosA * (t.x - c.x) - sinA * (t.y - c.y) + c.x),
//				(sinA * (t.x - c.x) + cosA * (t.y - c.y) + c.y)
//				);
//		}
		
		
		//-----------------------------------------------
		public static float Sine(float x)
		{
			float B = 4f /  Pi;
			float C = -4f / (Pi * Pi);
			float P = 0.225f;
			
			x = x % Pi;
			x = B * x + C * x * Math.Abs (x);
			x = P * (x * Math.Abs(x) - x) + x;
			
			return x;
		}
		
		static float PiOver2 = (float)Math.PI / 2f;
		static float TwoPi = (float)Math.PI*2f;
		public static float Cosine(float x)
		{
			x = x + PiOver2;
			x = x + (Math.Min(Math.Sign(Pi - x), 0) * TwoPi);
			return Sine(x);
		}
		/*
	public static void SinCosine(float v, out float sc)
	{
		 float B = 4f / Pi;
		 float C = -4f / (Pi * Pi);
		 float P = 0.225f;
 
		sc = float2(v % Pi, (v + PiOver2));
		sc.y = (sc.y + (Math.Min(Math.Sign(Pi - sc.y), 0f) * TwoPi)) % Pi;
 
		sc = B * sc + C * sc * Math.Abs(sc);
		sc = P * (sc * Math.Abs(sc) - sc) + sc;
	}
	public static void SinCosine(float v, out float x, out float y)
	{
		float sc;
		SinCosine(v, sc);	
		x = sc.x;
		y = sc.y;
	}
 	*/
		
	}
}

