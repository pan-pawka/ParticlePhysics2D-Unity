//Yves Wang @ FISH, 2015, All rights reserved
//Class that handles the branches 

using UnityEngine;

namespace ParticlePhysics2D {

	[System.Serializable]
	public class BinaryTree {
		
		// Variable definitions 
		float xPos;
		float yPos;
		float angle;
		float length;
		
		public int leafIndex;//the index of the particle created by this branch
		public int depth;//the depth of this branch in the binarytree
		
		[System.NonSerialized] public BinaryTree branchA;
		[System.NonSerialized] public BinaryTree branchB;
		[System.NonSerialized] public BinaryTree parent;
		
		static float TWO_PI = Mathf.PI * 2f;
		
		float minX = float.PositiveInfinity;
		float maxX = float.NegativeInfinity;
		float minY = float.PositiveInfinity;
		float maxY = float.NegativeInfinity;
		
		
		
		public static bool debugOn = true;
		public static Color pointColor = Color.green;
		public static Color branchColor = Color.yellow;
		public static int branchesCount = 0;
		
		public static float angleOffsetMin = 0.1f,angleOffsetMax = 0.5f;
		public static float lengthMin1 = 0.6f,lengthMax1 = 0.9f;
		public static float lengthMin2 = 0.5f,lengthMax2 = 0.7f;
		
		public static float lengthBranchAThreshold = 3f,lengthBranchBThreshold = 3f;
		public static int maxDepth = 9;
		public const int maxDepthLimit = 12;
		
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
				
			}
			minX = Mathf.Min(xB, minX);
			maxX = Mathf.Max(xB, maxX);
			minY = Mathf.Min(yB, minY);
			maxY = Mathf.Max(yB, maxY);
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
		
		// Render 
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
				if (debugOn) {
					Debug.DrawLine(thisPos,pos,branchColor);
				}
				if (branchA!=null) branchA.DebugRender(localToWorld);
				if (branchB!=null) branchB.DebugRender(localToWorld);
			} 
			//if it's a leaf branch
			else {
				Vector2 pos = new Vector2 (GetChildrenBranchPosX,GetChildrenBranchPosY);
				pos = (localToWorld==default(Matrix4x4)) ? pos : (Vector2)localToWorld.MultiplyPoint3x4(pos);
				if (debugOn) {
					Debug.DrawLine(thisPos,pos,branchColor);
					DebugExtension.DebugCircle(pos,Vector3.forward,Color.magenta,2f);
				}
				
			}
		}
				
	}
}

