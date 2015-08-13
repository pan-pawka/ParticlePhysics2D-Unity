//Yves Wang @ FISH, 2015, All rights reserved
//Class that handles the branches 

using UnityEngine;

namespace ParticlePhysics2D {
	public class Branch {
		
		// Variable definitions 
		float xPos;
		float yPos;
		float angle;
		public float length;
		
		public Branch branchA;
		public Branch branchB;
		public Branch parent;
		
		static float TWO_PI = Mathf.PI * 0.5f;
		
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
		public static float lengthMin2 = 0.1f,lengthMax2 = 0.5f;
		
		public static float lengthExit = 10f,lengthBranchAThreshold = 3f,lengthBranchBThreshold = 3f;
		public static float lengthExitRatio = 0.15f;
		
		const int maxBranchNum = 10000;
		
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
		
		// Constructor 
		public Branch (Branch parent, float x, float y, float angleOffset, float length) {
			Branch.branchesCount++;//need to set to 0 outside the Ctor
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
			
			//if this brancn has children branch,
			//limit it to maxBranchNum to prevent overshot
			if(length > lengthExit && Branch.branchesCount <= maxBranchNum) {
				
				if (length+Random.value * length  > lengthBranchAThreshold)
					branchA = new Branch(this, xB, yB, Random.Range(-angleOffsetMax,-angleOffsetMin) + ((angle % TWO_PI) < Mathf.PI ? -1f/length : +1f/length), length* Random.Range(lengthMin1,lengthMax1));
				else
					branchA = new Branch(this, xB, yB, Random.Range(-angleOffsetMax,-angleOffsetMin) + ((angle % TWO_PI) < Mathf.PI ? -1f/length : +1f/length), length* Random.Range(lengthMin2,lengthMax2));
				
				if (length+Random.value * length  > lengthBranchBThreshold)
					branchB = new Branch(this, xB, yB, Random.Range(angleOffsetMin,angleOffsetMax) + ((angle % TWO_PI) > Mathf.PI ? -1f/length : +1f/length), length * Random.Range(lengthMin1,lengthMax1));
				else 
					branchB = new Branch(this, xB, yB, Random.Range(angleOffsetMin,angleOffsetMax) + ((angle % TWO_PI) > Mathf.PI ? -1f/length : +1f/length), length * Random.Range(lengthMin2,lengthMax2));
			}
			//if this is a leaf branch
			else {
				
			}
			minX = Mathf.Min(xB, minX);
			maxX = Mathf.Max(xB, maxX);
			minY = Mathf.Min(yB, minY);
			maxY = Mathf.Max(yB, maxY);
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
			lengthMin2 = 0.1f; lengthMax2 = 0.5f;
			
			lengthBranchAThreshold = lengthBranchBThreshold = 3f;
			lengthExitRatio = 0.15f;
			lengthExit = lengthExitRatio * length;
		}
		
		// Render 
		public void DebugRender(Matrix4x4 localToWorld = default(Matrix4x4)) {
			
			Vector2 thisPos;
			if (localToWorld!=default(Matrix4x4))
				thisPos = localToWorld.MultiplyPoint3x4(this.Position);
			else thisPos = this.Position;
			
			if (debugOn) DebugExtension.DebugPoint(thisPos,pointColor);
			
			
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

