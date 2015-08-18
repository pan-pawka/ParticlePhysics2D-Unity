//Yves Wang @ FISH, 2015, All rights reserved

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ParticlePhysics2D {

	public class PCollision2DManager : Singleton<PCollision2DManager> {
		
		public static PCollision2DManager Instance {
			get {
				return ((PCollision2DManager)mInstance);
			} set {
				mInstance = value;
			}
		}
		
		const float fixedTimestep = 0.02f;
		const int maxProcessNumber = 10;
		
		public static List<ParticleCollision2D> collisionHolders = new List<ParticleCollision2D> ();
		
		//how many particle collider2d in total, on which we need to perform narrow phase
		static int narrowPhaseTotal {
			get {
				int t = 0;
				for (int i=0;i<collisionHolders.Count;i++) {
					t += collisionHolders[i].PCollider2DCount;
				}
				return t;
			}
		}
		
		// Use this for initialization
		void Start () {
			
		}
		
		int framesCount;
		int broadPhaseCount;
		int narrowPhaseCount;
		
		// Update is called once per frame
		void Update () {
		
			//how many frames we have in each fixed update
			framesCount = (int)(fixedTimestep * 1f / Time.deltaTime);
			
			//how many Broad Phase BVH update should we process in each update, a collision holder will have one BVH calculation
			broadPhaseCount = Mathf.Min ( Mathf.CeilToInt((float) collisionHolders.Count / (float) framesCount), maxProcessNumber );
			
			//how many narrow phase update should we perform in each update
			narrowPhaseCount = Mathf.Min( Mathf.CeilToInt((float) narrowPhaseTotal / (float) framesCount), maxProcessNumber );
			
		}
		
		void LateUpdate() {
			
		}
	}

}

