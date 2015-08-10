//Yves Wang @ FISH, 2015, All rights reserved
//Class that handles the branches 

using UnityEngine;


public class Branch {
      
    // Variable definitions 
    float xPos;
	float yPos;
	float angle;
	float length;
   
    public Branch branchA;
    public Branch branchB;
    public Branch parent;
    
    static float TWO_PI = Mathf.PI * 2f;
	
	float minX = float.PositiveInfinity;
	float maxX = float.NegativeInfinity;
	float minY = float.PositiveInfinity;
	float maxY = float.NegativeInfinity;
	
	public static bool debugOn = true;
	public static Color pointColor = Color.green;
	public static Color branchColor = Color.yellow;
	public static int branchesCount = 0;
	
	public static float angleOffsetMin,angleOffsetMax;
	public static float lengthMin1,lengthMax1;
	public static float lengthMin2,lengthMax2;
	
	public static float lengthExit,lengthBranchAThreshold,lengthBranchBThreshold;
	
	public Vector2 Position {
		get {
			return new Vector2 (xPos,yPos);
		}
	}
     
    // Constructor 
    public Branch (Branch parent, float x, float y, float angleOffset, float length) {
		branchesCount++;
        this.parent = parent;
        this.xPos = x;
        this.yPos = y;
        if(parent != null) {
            angle = parent.angle+angleOffset;
        } else {
            angle = angleOffset;
        }
        this.length = length;
        float xB = xPos + Mathf.Sin(angle) * length;
        float yB = yPos + Mathf.Cos(angle) * length;
		if(length > lengthExit) {
			if (length+Random.value * length  > lengthBranchAThreshold)
				branchA = new Branch(this, xB, yB, Random.Range(-angleOffsetMin,-angleOffsetMax) + ((angle % TWO_PI) > Mathf.PI ? -1f/length : +1f/length), length* Random.Range(lengthMin1,lengthMax1));
			else
				branchA = new Branch(this, xB, yB, Random.Range(-angleOffsetMin,-angleOffsetMax) + ((angle % TWO_PI) > Mathf.PI ? -1f/length : +1f/length), length* Random.Range(lengthMin2,lengthMax2));
				
			if (length+Random.value * length  > lengthBranchBThreshold)
				branchB = new Branch(this, xB, yB, Random.Range(angleOffsetMin,angleOffsetMax) + ((angle % TWO_PI) > Mathf.PI ? -1f/length : +1f/length), length * Random.Range(lengthMin1,lengthMax1));
			else 
				branchB = new Branch(this, xB, yB, Random.Range(angleOffsetMin,angleOffsetMax) + ((angle % TWO_PI) > Mathf.PI ? -1f/length : +1f/length), length * Random.Range(lengthMin2,lengthMax2));
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
    
	public static void ResetParams(){
		angleOffsetMin = 0.1f; angleOffsetMax = 0.5f;
		
		lengthMin1 = 0.6f; lengthMax1 = 0.9f;
		lengthMin2 = 0.1f; lengthMax2 = 0.5f;
		
		lengthExit = 10f; lengthBranchAThreshold = lengthBranchBThreshold = 3f;
    }
     
    // Render 
    public void DebugRender(Matrix4x4 localToWorld = default(Matrix4x4)) {
    
		Vector2 thisPos;
		if (localToWorld!=default(Matrix4x4))
			thisPos = localToWorld.MultiplyPoint3x4(this.Position);
		else thisPos = this.Position;
		
		if (debugOn) DebugExtension.DebugPoint(thisPos,pointColor);
		
    	//if it's not a leaf branch
        if(branchA != null) {
			Vector2 posA = (localToWorld==default(Matrix4x4)) ? branchA.Position : (Vector2)localToWorld.MultiplyPoint3x4(branchA.Position);
        	if (debugOn) {
        		Debug.DrawLine(thisPos,posA,branchColor);
        	}
			if (localToWorld==default(Matrix4x4)) 
				branchA.DebugRender();
			else branchA.DebugRender(localToWorld);
        }
        
		if(branchB != null) {
			Vector2 posB = (localToWorld==default(Matrix4x4)) ? branchB.Position : (Vector2)localToWorld.MultiplyPoint3x4(branchB.Position);
			if (debugOn) {
				Debug.DrawLine(thisPos,posB,branchColor);
			}
			if (localToWorld==default(Matrix4x4)) 
				branchB.DebugRender();
			else branchB.DebugRender(localToWorld);
		}
			
		//if it's leaf branch
        if (branchA==null && branchB == null) 
        {
			//draw leaf here
			if (debugOn) DebugExtension.DebugCircle(thisPos,Vector3.forward,Color.magenta,2f);
        }
    }
    
	
     
    
}