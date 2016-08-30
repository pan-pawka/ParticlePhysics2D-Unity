using UnityEngine;
using System.Collections;

//[RequireComponent(typeof(FFE_MoveControl))]
public class SpinalMove2 : MonoBehaviour {

	struct Bone {
		//static float boneLength;
		public Vector2 pos;//the position of the bone
		public Vector2 up;//the direction the bone is pointing,the length equals to the boneLength
	}

	public bool drawGizmo = false;

	[Range(3,20)]
	public int numberOfBones = 7;

	//The total length of the spinal creature
	[Range(1f,100f)]
	public float length = 7f;								

	//internally represent bones
	Bone[] bones;
	//internally represent the bone length
	float boneLength;

	//internally represent the minimal angle between each bone
	//the larger the stiffer
	[Range(120f,170f)]
	public float minBoneAngle = 120f;
	float minBoneRadian;

	//imagine a standard bone as a vec2 (0f,boneLength);
	//this two vector represent the min angled bone in its local space
	Vector2 stdBone;
	Vector2 minBoneL,minBoneR;

	// Use this for initialization
	void Start () {

		//create bones
		//once the bone params are set its not possible to change them during runtime
		boneLength = length / (float)numberOfBones;
		//minBoneL was get by rotating the standard vec counterclockwise by minBoneAngle.
		stdBone = new Vector2 (0f,boneLength);
		minBoneL = RotatePoint2D ( stdBone , Vector2.zero,  minBoneAngle );
		minBoneR = RotatePoint2D ( stdBone , Vector2.zero, -minBoneAngle );

		bones = new Bone[numberOfBones];
		Vector2 upVec = this.transform.up;
		bones[0].pos = (Vector2)this.transform.position - upVec * boneLength;
		bones[0].up = upVec;
		for (int i=1;i<numberOfBones;i++) {
			bones[i].pos = bones[i-1].pos - upVec * boneLength;
			bones[i].up = upVec;
		}


	}
	
	// Update is called once per frame
	void Update () {
		minBoneRadian = minBoneAngle * Mathf.Deg2Rad;
		Vector2 cusPos = (Vector2)this.transform.position;
		bones[0].pos = Drag (bones[0].pos,cusPos);
		bones[0].up = cusPos - bones[0].pos;
		for (int i=1;i<numberOfBones;i++) {
			//drag the bone to new pos
			bones[i].pos = Drag(bones[i].pos,bones[i-1].pos);
			bones[i].up = bones[i-1].pos - bones[i].pos;
			//test the min angle
			float radian = SignedRadian(-bones[i].up,bones[i-1].up);
			if (Mathf.Abs(radian) < minBoneRadian) {
				Debug.Log(radian);
				//rotate the minBone to the target bone's loacl space
				Vector2 _minBone;
				if (radian<0f) _minBone = minBoneL; else _minBone = minBoneR;
				//known that target bone has rotated from standard bone by some rotation,apply the rot to minBone
				_minBone = RotateByRef(stdBone,bones[i-1].up,_minBone);
				//get the angled bones position
				Vector2 np = bones[i-1].pos + _minBone - bones[i].pos;
				bones[i].pos += np * 0.5f;
				bones[i].up = bones[i-1].pos - bones[i].pos;
			}
		}
	}



	void OnDrawGizmos() {
		if (drawGizmo==false) return;

		//when script is running we draw the bones in real time
		if (bones!=null ) {
			Vector2 posP;
			posP = this.transform.position;
			Gizmos.color = Color.yellow;
			for (int i=0;i<bones.Length;i++) {
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(bones[i].pos,0.1f);
				Gizmos.color = Color.green;
				Gizmos.DrawLine(bones[i].pos,posP);
				posP = bones[i].pos;
			}
		}

		//when the script is not running we draw a straight line in editor
		if (Application.isPlaying==false) {
			//return;
			float boneL = length / numberOfBones;
			Vector2 posP,posN;
			posP = (Vector2)(this.transform.position);
			//Gizmos.DrawWireSphere(posP,0.1f);
			for (int i=0;i<numberOfBones;i++) {
				posN = posP - (Vector2)this.transform.up * boneL;
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(posN,0.1f);
				Gizmos.color = Color.green;
				Gizmos.DrawLine(posP,posN);
				posP = posN;
			}
		}
	}

	//drag a bone to the new position without considering minimal angle,as well as updating its up vec
	public Vector2 Drag(Vector2 bone,Vector2 pos) {
		Vector2 delta = pos - bone;
		float angle = Mathf.Atan2(delta.y,delta.x);
		Vector2 np = new Vector2 (
			pos.x - Mathf.Cos(angle) * boneLength,
			pos.y - Mathf.Sin(angle) * boneLength
		);
		return np;
	}

	public static Vector2 RotatePoint2D(Vector2 point,Vector2 axis,float angle) {
		angle *= Mathf.Deg2Rad; 
		float angleCos = Mathf.Cos(angle);
		float angleSin = Mathf.Sin(angle);
		float px = angleCos * (point.x - axis.x) - angleSin * (point.y - axis.y) + axis.x;
		float py = angleSin * (point.x - axis.x) + angleCos * (point.y - axis.y) + axis.y;
		return new Vector2 (px,py);
	}

	//known that vector a rotates to vector b, apply the same rotation to t
	//rotation matrix is 
	//[cos d,-sin d]
	//[sin d,cos d]
	//so b.x = cosd * a.x - sind * a.y;b.y = sind * a.x + cosd * a.y
	Vector2 RotateByRef(Vector2 a,Vector2 b,Vector2 t) {
		float asqr = a.x * a.x + a.y * a.y;
		float cos = ( b.x * a.x + b.y * a.y) / asqr;
		float sin = (a.x * b.y - a.y * b.x ) / asqr;
		return new Vector2 (
			cos * t.x - sin * t.y,
			sin * t.x + cos * t.y
		);
	}

	public static float SignedRadian (Vector2 a, Vector2 b) { 
		float a1 = Mathf.Atan2(a.y,a.x);
		float b1 = Mathf.Atan2(b.y,b.x);
		return b1 - a1;
	}

}

