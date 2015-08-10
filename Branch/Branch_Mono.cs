/// <summary>
/// Branch_ mono.
/// working in world co-ordinate
/// </summary>
using UnityEngine;
using System.Collections;
using ParticlePhysics2D;

[ExecuteInEditMode]
public class Branch_Mono : MonoBehaviour {

	[HideInInspector]
	public Branch branch;
	
	[HideInInspector]
	public Simulation sim;
	
	public float ks;
	
	[Range(0f,360f)]
	public float angle;
	
	[Range(20f,150f)]
	public float length;
	
	public bool debugBranch = false,debugParticlePhysics = false,debugIndex = false;
	
	//branch generation params
	[HideInInspector] public float lengthExit;
	[HideInInspector] public float angleOffsetMin,angleOffsetMax;
	[HideInInspector] public float lengthMin1,lengthMax1,lengthMin2,lengthMax2;
	[HideInInspector] public float lengthBranchAThreshold,lengthBranchBThreshold;
	
	void Start() {
		if (sim!=null) {
			if (sim.integrator==null) sim.setIntegrator();
		}
	}
	
	void Update () {
		//sim.tick();
	}
	
	void LateUpdate(){
		
	}
	
	//copy branch's topology to simulation
	void CopyBranchTopology(Particle2D p, Branch b,ref Simulation s) {
	
		if (b.branchA!=null || b.branchB!=null) {
			Particle2D temp = s.makeParticle(b.branchA.Position);
			s.makeSpring(p,temp,ks);
			if (b.branchA!=null) CopyBranchTopology (temp,b.branchA,ref s);
			if (b.branchB!=null) CopyBranchTopology (temp,b.branchB,ref s);
		}
		
	}
	
	//called by the editor script
	public void ReGenerateBranch(){
		Debug.Log(System.DateTime.Now);
		Branch.branchesCount = 0;
		//Branch.ResetParams();
		branch = new Branch (null,transform.position.x,transform.position.y,angle * Mathf.Deg2Rad,length);
		Debug.Log("Branches : " + Branch.branchesCount);
		if (sim==null)
			sim = new Simulation (IntegrationMedthod.VERLET);
		sim.setGravity(0f,0f);
		sim.clear();
		Particle2D start = sim.makeParticle (branch.Position);
		CopyBranchTopology(start,branch,ref sim);
		if (Application.isEditor) OnDrawGizmos();
	}
	
	void OnDrawGizmos() {
		//draw the axis here
		Vector3 end = (Vector2)transform.position + Mathp.RotateVector2(new Vector3 (0f,length,0f),-angle);
		Debug.DrawLine(transform.position,end,Color.magenta);
		
		if (debugBranch) {
			if (branch!=null) branch.DebugRender();
			else Debug.Log("branch is null");
		}
		if (debugParticlePhysics) {
			if (sim!=null) sim.DebugSpring();
		}
	}
}
