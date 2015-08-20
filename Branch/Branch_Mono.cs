/// <summary>
/// Yves Wang @ FISH, 2015, All rights reserved
/// Branch_ mono.
/// working in world co-ordinate
/// </summary>
using UnityEngine;
using System.Collections;
using ParticlePhysics2D;

[ExecuteInEditMode]
[AddComponentMenu("ParticlePhysics2D/Forms/BinaryTree",13)]
public class Branch_Mono : MonoBehaviour, IFormLayer {

	
	BinaryTree branch;
	[HideInInspector] [SerializeField] byte[] serializedBranch;
	public BinaryTree GetBinaryTree {get{return branch;}}
	
	[SerializeField] Simulation sim;
	public Simulation GetSimulation { get {return sim;}}
	
	[Range(20f,150f)]
	public float length = 20f;
	public float ks = 0.5f;
	
	public bool debugParticlePhysics = false,debugIndex = false;
	
	//branch generation params
	[HideInInspector] public float lengthExitRatio;
	[HideInInspector] public float angleOffsetMin,angleOffsetMax;
	[HideInInspector] public float lengthMin1,lengthMax1,lengthMin2,lengthMax2;
	[HideInInspector] public float lengthBranchAThreshold,lengthBranchBThreshold;
	[HideInInspector] public int maxDepth;
	[HideInInspector] public int leafCount;
	
	//some events
	public event System.Action OnResetForm;//called by editor to invoke form reset
	public event System.Action OnClearForm;
	
	
	void Awake() {
		Debug.Log("Re construct branch from serialized bytes");
		branch = EasySerializer.DeserializeObjectFromBytes(serializedBranch) as BinaryTree;
	}
	
	void LateUpdate(){
		OnDrawGizmosUpdate();
	}
	
	//copy branch's topology to simulation
	void CopyBranchTopology(Particle2D p, BinaryTree b,ref Simulation s) {
		
		//if the branch has children
		if (b.branchA!=null || b.branchB!=null) {
			Particle2D temp;
			temp = (b.branchA==null) ? s.makeParticle(b.branchB.Position) : s.makeParticle(b.branchA.Position);
			temp.IsLeaf = false;
			s.makeSpring(p,temp,ks);
			if (b.branchA!=null) CopyBranchTopology (temp,b.branchA,ref s);
			if (b.branchB!=null) CopyBranchTopology (temp,b.branchB,ref s);
		} 
		//if it's a leaf branch
		else {
			float xB = b.GetChildrenBranchPosX;
			float yB = b.GetChildrenBranchPosY;
			Particle2D temp = s.makeParticle(new Vector2(xB,yB));//temp is where the leaf is
			temp.IsLeaf = true;
			leafCount++;
			s.makeSpring(p,temp,ks);
		}
		
	}
	
	//called by the editor script
	public void ResetForm(){
	
		branch = BinaryTree.GenerateBranch(length);
		//Debug.Log("Branches : " + BinaryTree.branchesCount);
		if (sim==null)
			sim = new Simulation (IntegrationMedthod.VERLET);
		sim.setGravity(0f,0f);
		sim.clear();
		leafCount = 0;
		Particle2D start = sim.makeParticle (branch.Position);
		CopyBranchTopology(start,branch,ref sim);
		if (Application.isEditor) OnDrawGizmosUpdate();
		Debug.Log("Serialize branch to bytes");
		serializedBranch = EasySerializer.SerializeObjectToBytes(branch);
		
		if (OnResetForm!=null) OnResetForm();//invokde the event
		else {
			//Debug.Log("No one register OnResetForm");
		}
	}	
	
	public void ClearForm() {
		sim.clear();
		sim.clearForces();
		this.branch = null;
		this.leafCount = 0;
		if (OnClearForm!=null) OnClearForm();//invoke the clear form event
		else {
			//Debug.Log("No one register OnClearForm");
		}
	}
	
	public void OnDrawGizmosUpdate() {
		if (debugParticlePhysics) {
			if (sim!=null) sim.DebugSpring(transform.localToWorldMatrix);
		}
		if (BinaryTree.debugBranch) {
			if (branch!=null) branch.DebugRender(transform.localToWorldMatrix);
			else Debug.Log("branch is null");
		}
	}
	
	//a gizmo that is handy to pick up
	 void OnDrawGizmos() {
		Gizmos.DrawSphere(transform.position,5f);
		
	}
	
	void OnDestroy(){

	}
	
	
	
	
}
