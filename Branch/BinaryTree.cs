//Yves Wang @ FISH, 2015, All rights reserved
//Class that handles the branches 

using UnityEngine;
using UnityEngineInternal;

namespace ParticlePhysics2D {

	//bounding circle struct
	public struct BoundingCircle {
		public float radius;
		public Vector2 position;
		public BoundingCircle(Vector2 position, float radius) {
			this.position = position;
			this.radius = radius;
		}
		public static BoundingCircle zero {
			get {
				return new BoundingCircle (Vector2.zero,0f);
			}
		}
		//todo: optimize: do not instantiate new one
		public static BoundingCircle GetFromTwo(BoundingCircle a,BoundingCircle b) {
			Vector2 d = a.position - b.position;
			float dist = d.magnitude;
			float r = (dist + a.radius + b.radius) /2f;
			return new BoundingCircle  (b.position + d * (r - b.radius) / dist , r );
		}
		public bool IsZero {
			get {
				if (this.position==Vector2.zero && this.radius == 0f) return true;
				else return false;
			}
		}
		public bool Overlaps(Vector2 pos, float radius) {
			float m2 = (pos - this.position).sqrMagnitude;
			float Rr = radius + this.radius;
			if (m2 < Rr * Rr ) return true; else return false;
		}
		
		//if the testing boudning circle overlpas with the tested circle,
		//tested should be move by dir, and tesing one should be move by -dir, in order to satisfy.
		public bool OverlapsResults(Vector2 pos, float radius, out Vector2 dir) {
			dir = pos - this.position;
			float m2 = dir.sqrMagnitude;
			float Rr = radius + this.radius;
			if (m2 > Rr * Rr) return false; else {
				m2 = Mathf.Sqrt(m2);
				float o = radius + this.radius - m2;
				dir *= (o/m2);
				return true;
			}
		}
		
		public void DebugDraw(Matrix4x4 local2World,int depth, Color color) {
			Vector2 pos = local2World.MultiplyPoint3x4(this.position);
			DebugExtension.DebugCircle(pos,Vector3.forward,color * (float)depth / 8f,radius);
		}
	}

	[System.Serializable]
	public class BinaryTree {
	
		//the leaf bounding radius for collison detection
		[System.NonSerialized]
		public BoundingCircle boundingCircle = BoundingCircle.zero;
		public BoundingCircle GetBoundingCircle (Simulation sim,float leafColliderRadius){
			if (branchA==null && branchB==null) {
				this.boundingCircle = new BoundingCircle (sim.getParticlePosition(leafIndex),leafColliderRadius);
				return this.boundingCircle;
			} else {
				this.boundingCircle = BoundingCircle.GetFromTwo(branchA.GetBoundingCircle(sim,leafColliderRadius),branchB.GetBoundingCircle(sim,leafColliderRadius));
				return this.boundingCircle;
			}
		}
		
		// Variable definitions 
		[System.NonSerialized] float xPos;
		[System.NonSerialized] float yPos;
		[System.NonSerialized] float angle;
		[System.NonSerialized] float length;
		
		//the index of the leaf particle in the simulation instance
		//note that the index of the corresponding spring2d, is leafIndex - 1;
		public int leafIndex;
		public int springIndex {get {return leafIndex - 1;}}
		public int depth;//the depth of this branch in the binarytree
		
		public BinaryTree branchA;
		public BinaryTree branchB;
		public BinaryTree parent;
		
		static float TWO_PI = Mathf.PI * 2f;
//		
//		[System.NonSerialized] float minX = float.PositiveInfinity;
//		[System.NonSerialized] float maxX = float.NegativeInfinity;
//		[System.NonSerialized] float minY = float.PositiveInfinity;
//		[System.NonSerialized] float maxY = float.NegativeInfinity;
//		
//		
		
		public static int branchesCount = 0;
		
		public static float angleOffsetMin = 0.1f,angleOffsetMax = 0.5f;
		public static float lengthMin1 = 0.6f,lengthMax1 = 0.9f;
		public static float lengthMin2 = 0.5f,lengthMax2 = 0.7f;
		
		public static float lengthBranchAThreshold = 3f,lengthBranchBThreshold = 3f;
		public static int maxDepth = 9;
		public const int maxDepthLimit = 12;
		
		//global debug option
		public static Color pointColor = Color.green;
		public static Color branchColor = Color.yellow - new Color (0f,0f,0f,0.3f);
		public static Color leafColor = Color.magenta - new Color (0f,0f,0f,0.3f);
		public static Color boundingCircleColor = Color.cyan - new Color (0f,0f,0f,0.3f);
		
		public static bool debugBranch = false,debugBranchLeaf = false, debugBranchBoundingCircle = false;
		public static int debugBoundingCircleDepth = 0;
		
		public Vector2 Position {
			get {
				return new Vector2 (xPos,yPos);
			}
		}
		
		public float GetChildrenBranchPosX {
			get {
				return xPos + Mathf.Sin(angle) * length;
			}
		}
		
		public float GetChildrenBranchPosY {
			get {
				return yPos + Mathf.Cos(angle) * length;
			}
		}
		
		public static BinaryTree GenerateBranch (float length) {
			BinaryTree.branchesCount = 0;
			return new BinaryTree (null,0f,0f,0f,length,0);
		}
		
		// private Constructor 
		private BinaryTree (BinaryTree parent, float x, float y, float angleOffset, float length, int depth) {
			BinaryTree.branchesCount++;
			this.leafIndex = BinaryTree.branchesCount;
			this.depth = depth + 1;
			this.parent = parent;
			this.xPos = x;
			this.yPos = y;
			if(parent != null) {
				angle = parent.angle+angleOffset;
			} else {
				angle = angleOffset;
			}
			this.length = length;
			float xB = GetChildrenBranchPosX;
			float yB = GetChildrenBranchPosY;
			
			if(this.depth <= BinaryTree.maxDepth ) {

				if (length * Random.Range(1f,2f)  > lengthBranchAThreshold)
					branchA = new BinaryTree(this, xB, yB,  AngleOffsetA, length* Random.Range(lengthMin1,lengthMax1),this.depth);
				else
					branchA = new BinaryTree(this, xB, yB,  AngleOffsetA, length* Random.Range(lengthMin2,lengthMax2),this.depth);
				
				if (length * Random.Range(1f,2f)  > lengthBranchBThreshold)
					branchB = new BinaryTree(this, xB, yB,  AngleOffsetB, length * Random.Range(lengthMin1,lengthMax1),this.depth);
				else 
					branchB = new BinaryTree(this, xB, yB,  AngleOffsetB, length * Random.Range(lengthMin2,lengthMax2),this.depth);
			}
			//if this is a leaf branch
			else {
				branchA = branchB = null;
				
			}
//			minX = Mathf.Min(xB, minX);
//			maxX = Mathf.Max(xB, maxX);
//			minY = Mathf.Min(yB, minY);
//			maxY = Mathf.Max(yB, maxY);
		}
		
		float AngleOffsetA {
			get {
				//angle is always negative
				return Random.Range(-angleOffsetMin,-angleOffsetMax) 
						+ ((angle % TWO_PI) > -Mathf.PI ? -1f/length : +1f/length) * 0.5f
						;
			}
		}
		
		float AngleOffsetB {
			get {
				//angle is always positive
				return Random.Range(angleOffsetMin,angleOffsetMax) 
						+ ((angle % TWO_PI) > Mathf.PI ? -1f/length : +1f/length) * 0.5f
						;
			}
		}
		
		float LengthNoise ( float f) {
			return Mathf.PerlinNoise(0f,length * f);
		}
		
		float PositionNoise(float f) {
			return Mathf.PerlinNoise(xPos*f,yPos*f);
		}
		
		// Set scale 
		void setScale(float scale) {
			length *= scale;
			if(branchA != null) branchA.setScale(scale);
			if(branchB != null) branchB.setScale(scale);
		}
		
		public static void ResetParams(float length){
			angleOffsetMin = 0.1f; angleOffsetMax = 0.5f;
			
			lengthMin1 = 0.6f; lengthMax1 = 0.9f;
			lengthMin2 = 0.5f; lengthMax2 = 0.7f;
			
			lengthBranchAThreshold = lengthBranchBThreshold = 3f;
			//lengthExitRatio = 0.15f;
			//lengthExit = lengthExitRatio * length;
			maxDepth = 6;
		}
		
		// Render the branch in unity editor for debug purpose
		// boundingCircleDepth = -1 means draw all of them
		public void DebugRender(Matrix4x4 localToWorld = default(Matrix4x4)) {
			
			Vector2 thisPos;
			if (localToWorld!=default(Matrix4x4))
				thisPos = localToWorld.MultiplyPoint3x4(this.Position);
			else thisPos = this.Position;
			
			//if (debugOn) DebugExtension.DebugPoint(thisPos,pointColor);
			
			
			//if it's not a leaf branch
			if (branchA!=null || branchB!=null) {
				Vector2 pos = (branchA==null) ? branchB.Position : branchA.Position;
				pos = (localToWorld==default(Matrix4x4)) ? pos : (Vector2)localToWorld.MultiplyPoint3x4(pos);
				
				Debug.DrawLine(thisPos,pos,branchColor);
				
				if (branchA!=null) branchA.DebugRender(localToWorld);
				if (branchB!=null) branchB.DebugRender(localToWorld);
			} 
			//if it's a leaf branch
			else {
				Vector2 pos = new Vector2 (GetChildrenBranchPosX,GetChildrenBranchPosY);
				pos = (localToWorld==default(Matrix4x4)) ? pos : (Vector2)localToWorld.MultiplyPoint3x4(pos);
				Debug.DrawLine(thisPos,pos,branchColor);
				if (BinaryTree.debugBranchLeaf) DebugExtension.DebugCircle(pos,Vector3.forward,leafColor,length/10f);
			}
			
			//debug bounding circle
			if (BinaryTree.debugBranchBoundingCircle)
			if (boundingCircle.IsZero == false && (BinaryTree.debugBoundingCircleDepth == -1 || BinaryTree.debugBoundingCircleDepth == depth)) {
				Vector2 p = boundingCircle.position;
				p = (localToWorld==default(Matrix4x4)) ? p : (Vector2)localToWorld.MultiplyPoint3x4(p);
				DebugExtension.DebugCircle(p,Vector3.forward,boundingCircleColor, boundingCircle.radius);
			}
		}
				
	}
}

