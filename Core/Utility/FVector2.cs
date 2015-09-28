using UnityEngine;
using System.Collections;
using System;

[Serializable]
public struct FVector2 {

	static float rad2deg = 180f / (float) Math.PI;
	public static float TWOPI = (float)Math.PI * 2f;
	
	public float x,y;
	public static FVector2 zero = new FVector2 (0f,0f);
	public static FVector2 one = new FVector2 (1f,1f);
	public static FVector2 up = new FVector2 (0f,1f);
	public static FVector2 down = new FVector2 (0f,-1f);
	public static FVector2 left = new FVector2 (-1f,0f);
	public static FVector2 right = new FVector2 (1f,0f);
	
	public FVector2(float x, float y) {this.x = x;this.y = y;}
	public FVector2(Vector3 p) {this.x = p.x;this.y = p.y;}
	public FVector2(Vector2 p) {this.x = p.x;this.y = p.y;}
	
	public float sqrMagnitude { get { return this.x * this.x + this.y * this.y; } }
	public float Magnitude { get { return (float)Math.Sqrt(this.x * this.x + this.y * this.y); } }
	public FVector2 normalized { get { float l = Magnitude; return new FVector2 (this.x/l,this.y/l);} }
	
	public static FVector2 operator + (FVector2 a , FVector2 b) {return new FVector2 (a.x + b.x,a.y + b.y);}
	public static FVector2 operator - (FVector2 a , FVector2 b) {return new FVector2 (a.x - b.x,a.y - b.y);}
	public static bool operator == (FVector2 a , FVector2 b) {return a.x == b.x && a.y == b.y;}
	public static bool operator != (FVector2 a , FVector2 b) {return a.x != b.x || a.y != b.y;}
	//public override bool Equals ( FVector2 a ) {return this.x == a.x && this.y == a.y;}
	
	public static implicit operator FVector2 (Vector2 p) {return new FVector2(p.x,p.y);}
	public static implicit operator FVector2 (Vector3 p) {return new FVector2(p.x,p.y);}
	public static implicit operator Vector2 (FVector2 p) {return new Vector2(p.x,p.y);}
	public static implicit operator Vector3 (FVector2 p) {return new Vector3(p.x,p.y,0f);}
	
	//product
	public static float operator * (FVector2 a , FVector2 b) {return a.x * b.y - a.y * b.x;}//Dot Product
	public static float operator * (FVector2 a , Vector2 b) {return a.x * b.y - a.y * b.x;}//Dot Product
	public static float operator * (Vector2 a , FVector2 b) {return a.x * b.y - a.y * b.x;}//Dot Product
	public static float Cross (FVector2 a, FVector2 b) {return a.x * b.y - a.y * b.x;}//Corss Product
	public static float Cross (Vector2 a, Vector2 b) { return a.x * b.y - a.y * b.x;}//Corss Product
	public static float Cross (FVector2 a, Vector2 b) { return a.x * b.y - a.y * b.x;}//Corss Product
	public static float Cross (Vector2 a, FVector2 b) { return a.x * b.y - a.y * b.x;}//Corss Product
	public float Cross (FVector2 a) { return this.x * a.y - this.y * a.x;}
	public float Cross (Vector2 a) { return this.x * a.y - this.y * a.x;}
	
	//Cross with unit z
	public static Vector2 CrossPositiveZ(Vector2 a) {return new Vector2 (a.y,-a.x);}
	public static Vector2 CrossNegativeZ(Vector2 a) {return new Vector2 (-a.y,a.x);}
	public static Vector2 RotateCW90(Vector2 a) {return CrossPositiveZ(a);}
	public static Vector2 RotateCCW90(Vector2 a) {return CrossNegativeZ(a);}
	public static FVector2 CrossPositiveZ(FVector2 a) {return new FVector2 (a.y,-a.x);}
	public static FVector2 CrossNegativeZ(FVector2 a) {return new FVector2 (-a.y,a.x);}
	public static FVector2 RotateCW90(FVector2 a) {return CrossPositiveZ(a);}
	public static FVector2 RotateCCW90(FVector2 a) {return CrossNegativeZ(a);}
	public static Vector2 CrossUnitZ(Vector2 a,float unitZSign) {
		int sign = Math.Sign(unitZSign);
		return new Vector2 (a.y * sign,-a.x * sign);
	}
	
	
	public float Dot (FVector2 a) { return this.x * a.x + this.y * a.y;}
	public static float Dot (FVector2 a, FVector2 b) {return a.x * b.x + a.y * b.y;}
	public static float Dot (Vector2 a, Vector2 b) {return a.x * b.x + a.y * b.y;}
	
	public static float Distance ( FVector2 a, FVector2 b) {
		float xl = a.x - b.x;
		float yl = a.y - b.y;
		return (float)Math.Sqrt(xl * xl + yl * yl);
	}
	
	public void Set (float newX,float newY) {this.x = newX;this.y = newY;}
	
	#region Get Angle
	
	private static float signedAngle(float ax, float ay, float bx, float by) {
		double a1 = Math.Atan2(ay,ax);
		double b1 = Math.Atan2(by,bx);
		double r = b1 - a1;
		return (float)r;
	}
	
	/// <summary>
	/// Return the signed angle between two vectors.
	/// </summary>
	/// <returns>The signed angle.</returns>
	/// <param name="a">The vector a.</param>
	/// <param name="b">The vector b.</param>
	public static float SignedAngle (FVector2 a, FVector2 b) {
		return signedAngle(a.x,a.y,b.x,b.y) * rad2deg;
	}
	
	public static float SignedAngle (Vector2 a, Vector2 b) { 
		return signedAngle(a.x,a.y,b.x,b.y) * rad2deg;
	}
	
	public static float SignedAngleRadian (Vector2 a, Vector2 b) { 
		return signedAngle(a.x,a.y,b.x,b.y);
	}
	
	public static float SignedAngleRadian (FVector2 a, FVector2 b) { 
		return signedAngle(a.x,a.y,b.x,b.y);
	}
	
	public static float UnsignedAngle (FVector2 a, FVector2 b) {
		float r = SignedAngle(a,b);
		return (r < 0f ) ? -r : r;
	}
	
	public static float UnsignedAngle (Vector2 a, Vector2 b) {
		float r = SignedAngle(a,b);
		return (r < 0f ) ? -r : r;
	}
	#endregion
	
	public override string ToString() {
		return "X = "+this.x+" Y = "+this.y;
	}
}
