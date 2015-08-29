using UnityEngine;
using System.Collections;
using System;

[ExecuteInEditMode]
public class AngleCosTest : MonoBehaviour {


	public Transform a,b,m;
	public Transform ap,bp;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log(GetAngleCos(a.position,b.position));

		float cos1 = GetAngleCos(a.position,b.position);
		float cos2 = GetAngleCos(ap.position,bp.position);

		float deltaCos = (cos2 - cos1)/2f;
		Debug.Log(deltaCos);
		
		GetDeltaA(deltaCos);
	}
	
	public float GetAngleCos (Vector2 apos,Vector2 bpos){
		Vector2 mpos = m.position;
		float lengtha = (apos - mpos).magnitude;
		float lengthb = (bpos - mpos).magnitude;
		float lengthMultiplar = lengtha * lengthb;
		Vector2 av = apos - mpos;
		Vector2 bv = bpos - mpos;
		float dotP = FVector2.Dot(av,bv);
		float crossP = FVector2.Cross(av,bv);
		return (dotP/lengthMultiplar + 1f) * Math.Sign(crossP);
	}
	
	
	
	public void GetDeltaA (float deltaC) {
		Vector2 va = ap.position - m.position;
		Vector2 vb = bp.position - m.position;
		//float zunit = FVector2.Cross(va,vb);
		float zunit = deltaC;
		
		Vector2 deltaA = FVector2.CrossUnitZ(va,zunit) * Mathf.Abs(deltaC) / 2f;
		Vector2 deltaB = FVector2.CrossUnitZ(vb,-zunit)* Mathf.Abs(deltaC) / 2f;
		DebugExtension.DebugArrow(ap.position,deltaA);
		DebugExtension.DebugArrow(bp.position,deltaB);
	}

	void OnDrawGizmos () {
	
		Gizmos.DrawLine(a.position,m.position);
		Gizmos.DrawLine(b.position,m.position);
		
		Gizmos.color = Color.green;
		
		Gizmos.DrawLine(ap.position,m.position);
		Gizmos.DrawLine(bp.position,m.position);
		
		
	}
}
