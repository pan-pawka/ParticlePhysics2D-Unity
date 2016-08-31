//Yves Wang @ FISH, 2015, All rights reserved
using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace ParticlePhysics2D {

	public enum IntegrationMedthod {Verlet, GPUVerlet}

	[System.Serializable]
	public class Simulation  : ISerializationCallbackReceiver {

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		/// Fields and Properties
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		
		[HideInInspector] [SerializeField] List <Particle2D> particles;
		
		[HideInInspector] [SerializeField] List <Spring2D> springs;
		
		[HideInInspector] [SerializeField] List<AngleConstraint2D> angles;
		
		public bool overrideGlobalSetting;
		
		public SimSettings SettingsSlot;//this is only for users to assign in editor
		public SimSettings Settings{get{return _Settings;}}
		SimSettings _Settings;//internally we use this one
		
		private IntegratorBase _integrator;
		public IntegratorBase Integrator {
			get {
				if (_integrator!=null) return _integrator;
				else {
					setIntegrator();
					return _integrator;
				}
			}
		}
		
		[SerializeField]
		[ReadOnlyAttribute]
		public int maxSpringConvergenceID = 0,maxAngleConvergenceID = 0;
		
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		/// Methods
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		
		public void setIntegrator()
		{
			switch ( _Settings.integrationMethod )
			{
			case IntegrationMedthod.Verlet:
				this._integrator = new VerletIntegrator(this) as IntegratorBase;
				break;
			case IntegrationMedthod.GPUVerlet:
				this._integrator = new GPUVerletIntegrator(this) as IntegratorBase;
				break;
			//case IntegrationMedthod.ThreadedVerlet:
			//	this._integrator = new ParallelProcessIntegrator(this) as IntegratorBase;
			//	break;
			default:
				break;
			}
			
		}
		
		/// <summary>
		/// this deletes all the particles and all the forces in the system
		/// clear all the primitive data in the simulation
		/// </summary>
		public void clear()
		{
			particles.Clear();
			springs.Clear();
			angles.Clear();
		}
		
		/// <summary>
		/// Init this instance. Call this inside Start() to instantiate the integrator
		/// otherwise, it'll be created in the Property in the first frame
		/// </summary>
		public void Init () {
			_Settings = SettingsSlot;
			if (!overrideGlobalSetting) _Settings = SimulationManager.Instance.settings;
			else {
				if (SettingsSlot == null) _Settings = SimulationManager.Instance.settings;
			}
			if (Application.isPlaying) setIntegrator();
			else return;
		}
		
		#region Convergence Group ID
		/// <summary>
		/// Recalculates the convergence group according to the topology of the data form
		/// use this method after you have set up the data form as a meaningful shape.
		/// a convergence group id will be assgined to spring2d and angle2d.
		/// then in gpu solver, springs or angles in a same group will be made into a same RT.
		/// Note that it is best to shuffle the spring and angle list after you complete this method.
		/// otherwise I dont know if this method will return a valida result. However if you don't need GPU solver
		/// then this method is of no use.
		/// </summary>
		public void RecalculateConvergenceGroupID () {
			this.maxAngleConvergenceID = this.maxSpringConvergenceID = 0;
			int pNum = this.numberOfParticles();
			for (int i=0;i<pNum;i++) {
				this.CalcConvergenceID(i);
			}
		}
		
		/// <summary>
		/// Calculates the convergence ID of the springs and angles that are connected to particle specified by particleIndex
		/// </summary>
		/// <param name="particleIndex">Particle index.</param>
		private void CalcConvergenceID(int particleIndex) {
			Particle2D pp = this.getParticle(particleIndex);
			
			//Spring
			List<Spring2D> sp = new List<Spring2D> (10);//the conencted spring for particle index
			int springNum = this.numberOfSprings();
			//get the connected spring
			for (int i=0;i<springNum;i++) {
				Spring2D s = this.getSpring(i);
				Particle2D a = s.ParticleA;
				Particle2D b = s.ParticleB;
				if (a == pp || b == pp) {
					sp.Add(s);
				}
			}
			//calc the convergence id for the springs which connects to this particle
			List<int> IDinUse = new List<int> (10);
			for (int i=0;i<sp.Count;i++) if (sp[i].convergenceGroupID != 0) IDinUse.Add(sp[i].convergenceGroupID);
			for (int i=0;i<sp.Count;i++) {
				if (sp[i].convergenceGroupID == 0) {
					int id = 1;
					while (IDinUse.Contains(id)) id++;
					sp[i].convergenceGroupID = id;
					IDinUse.Add(id);
					if (id>this.maxSpringConvergenceID) this.maxSpringConvergenceID = id;
				}
			}
			
			//angle
			List<AngleConstraint2D> ag = new List<AngleConstraint2D> (10);
			int angleNum = this.numberOfAngleConstraints();
			//get the connected angles
			for (int i=0;i<angleNum;i++) {
				AngleConstraint2D angle = this.getAngleConstraint(i);
				Particle2D b = angle.ParticleB;
				Particle2D m = angle.ParticleM;
				if (b == pp || m == pp) ag.Add(angle);
			}
			//recalc the convergence id for the angles that are connected to this particle
			IDinUse.Clear();
			for (int i=0;i<ag.Count;i++) if (ag[i].convergenceGroupID != 0) IDinUse.Add(ag[i].convergenceGroupID);
			for (int i=0;i<ag.Count;i++) {
				if (ag[i].convergenceGroupID == 0) {
					int id = 1;
					while (IDinUse.Contains(id)) id++;
					ag[i].convergenceGroupID = id;
					IDinUse.Add(id);
					if (id>this.maxAngleConvergenceID) this.maxAngleConvergenceID = id;
				}
			}
		}
		
		/// <summary>
		/// Numbers of the springs by conv ID.
		/// </summary>
		/// <returns>The number of springs by conv ID.</returns>
		/// <param name="convID">Convergence Group ID.</param>
		public int numberOfSpringsByConvID(int convID)
		{
			if (convID ==0) return numberOfSprings(); 
			else if (convID > maxSpringConvergenceID) {
				Debug.LogError("The input convID is larger that the max convID");
				return 0;
			} else {
				int num = 0;
				for (int i=0;i<springs.Count;i++) {
					if (springs[i].convergenceGroupID == convID) num++;
				}
				return num;
			}
		}
		
		/// <summary>
		/// Numbers of the angles by conv ID.
		/// </summary>
		/// <returns>The number of angles by conv ID.</returns>
		/// <param name="convID">Conv Group ID.</param>
		public int numberOfAnglesByConvID(int convID)
		{
			if (convID ==0) return numberOfAngleConstraints(); 
			else if (convID > maxAngleConvergenceID) {
				Debug.LogError("The input convID is larger that the max convID");
				return 0;
			} else {
				int num = 0;
				for (int i=0;i<angles.Count;i++) {
					if (angles[i].convergenceGroupID == convID) num++;
				}
				return num;
			}
		}
		#endregion
		
		#region Serialization
		public void OnBeforeSerialize()  {}
		//this is a hack, because unity does not serialize custom class properly. Mainly because of ref lost.
		public void OnAfterDeserialize() {
			for (int i=0;i<springs.Count;i++) {
				springs[i].SetParticles(this);
			}
			for (int i=0;i<angles.Count;i++) {
				angles[i].SetParticles(this);
			}
			
		}
		#endregion
		
		#region Mesh Helper
		
		public int[] getIndices(){
			if (particles.Count<2 || springs.Count<1) return null;
			else {
				List<int> indices = new List<int> ((particles.Count-1)*particles.Count);//for a sim with n particles, maximum （n-1)*n/2  edges,indices double
				for (int i=0;i<springs.Count;i++) {
					indices.Add(particles.IndexOf(springs[i].ParticleA));
					indices.Add(particles.IndexOf(springs[i].ParticleB));
				}
				return indices.ToArray();
			}
		}
		
		public Vector3[] getVertices(){
			if (particles.Count<2) {
				return null;
			} else {
				Vector3[] vertices = new Vector3[particles.Count] ;
				for (int i=0;i<particles.Count;i++) {
					vertices[i] = particles[i].Position;
				}
				return vertices;
			}
		}
		
		private Vector3[] _verticesNonAlloc = new Vector3[0] ;
		public Vector3[] getVerticesNonAlloc(){
			if (particles.Count<2) {
				return null;
			} else {
				if (_verticesNonAlloc.Length != particles.Count) {
					System.Array.Resize<Vector3>(ref _verticesNonAlloc,particles.Count);
				}
				for (int i=0;i<particles.Count;i++) {
					_verticesNonAlloc[i] = particles[i].Position;
				}
				return _verticesNonAlloc;
			}
		}
		#endregion
		
		#region Tick
		
		/// <summary>
		/// advance the simulation by some time t, or by the default 1.0. 
		/// You'll want to call this in Update(). You probably want to keep this the same at all times 
		/// unless you want speed up or slow things down.
		/// </summary>s
		public void tick()
		{
			if (Application.isPlaying) this.Integrator.step();
			else return;
		}
		
		#endregion
		
		#region Particles
		public Particle2D makeParticle(  float x, float y)
		{
			return makeParticle(new Vector2(x,y));
		}
		
		public Particle2D makeParticle(  Vector2 pos)
		{
			Particle2D p = new Particle2D( );
			p.Position = pos;
			p.PositionOld = pos;
			particles.Add( p );
			return p;
		}
		
		public Particle2D makeParticle()
		{  
			return makeParticle( 0f, 0f );
		}
		
		public int numberOfParticles()
		{
			return particles.Count;
		}
		
		public Particle2D getParticle( int i )
		{
			//i = Mathf.Clamp(i,0,particles.Count-1);
			return particles[i];
		}
		
		public int getParticleIndex(Particle2D p) {
			for (int i=0;i<numberOfParticles();i++) {
				if (particles[i] == p) {
					//Debug.Log("YYY");
					return i;
				}
			}
			//Debug.LogError("Cannot find particle for index");
			return -1;
			
		}
		
		//this is a hacky method, used by serialization callback to rebuild the spring's references to its end particles.
		public Particle2D getParticleByPosition(Vector2 pos) {
			for (int i=0;i<particles.Count;i++) {
				if (particles[i].Position == pos) return particles[i];
			}
			return null;
		}
		
		public Vector2 getParticlePosition(int index) {
			if (index>=0 && index <particles.Count) {
				return particles[index].Position;
			} else return Vector2.zero;
		}
		
		public List<Particle2D> getLeafParticles() {
			List<Particle2D> lf = new List<Particle2D> (particles.Count / 2 + 1);
			for (int i=0;i<particles.Count;i++) {
				if (particles[i].IsLeaf) lf.Add(particles[i]);
			}
			return lf;
		}
		
		public Vector2 getParticlesCenter(){
			Vector2 c = Vector2.zero;
			for (int i=0;i<numberOfParticles();i++) {
				c += getParticle(i).Position;
			}
			return c/numberOfParticles();
		}
		
		public void removeParticle( Particle2D p )
		{
			for (int i=springs.Count-1;i>=0;i--) {
				if (springs[i].ParticleA == p || springs[i].ParticleB == p) {
					Spring2D s = springs[i];
					removeSpring(s);
				}
			}
			
			particles.Remove( p );
		}
		
		
		
		#endregion
		
		#region Spring
		
		public Spring2D makeSpring( Particle2D a, Particle2D b)
		{
			float r = Vector2.Distance(a.Position,b.Position);
			Spring2D s = new Spring2D( this , a, b, r );
			springs.Add( s );
			return s;
		}
		
		public int numberOfSprings()
		{
			return springs.Count;
		}

		public Spring2D getSpring( int i )
		{
			return springs[i];
		}
		
		public void removeSpring( Spring2D a )
		{
			for (int i=angles.Count-1;i>=0;i--) {
				if (angles[i].ContainSpring(a)) angles.RemoveAt(i);
			}
			springs.Remove( a );
		}
		
		public void ShuffleSprings() {
			springs.Shuffle();
		}
		
		#endregion
		
		#region Angle Constraints
		public AngleConstraint2D makeAngleConstraint( Spring2D s1, Spring2D s2)
		{
			AngleConstraint2D angle = new AngleConstraint2D (this,s1,s2);
			angles.Add( angle );
			return angle;
		}
		
		public int numberOfAngleConstraints()
		{
			return angles.Count;
		}

		public AngleConstraint2D getAngleConstraint( int i )
		{
			return angles[i];
		}
		
		public void removeAngleConstraint( int i )
		{
			angles.RemoveAt( i );
		}
		
		public void removeAngleConstraint( AngleConstraint2D a )
		{
			angles.Remove( a );
		}
		
		public void ShuffleAngles(){
			angles.Shuffle();
		}
		#endregion
		
		#region Simulation Constructor
		
		public Simulation()
		{
			particles = new List<Particle2D> ();
			springs = new List<Spring2D> ();
			angles = new List<AngleConstraint2D> ();
		}
		
//		public Simulation (float g,IntegrationMedthod integrationMedthod) {
//			particles = new List<Particle2D> ();
//			springs = new List<Spring2D> ();
//			angles = new List<AngleConstraint2D> ();
//		}
//		
//		public Simulation (IntegrationMedthod integrationMedthod) {
//			particles = new List<Particle2D> ();
//			springs = new List<Spring2D> ();
//			angles = new List<AngleConstraint2D> ();
//		}
		
		#endregion
		
		#region Debug
		
		public void DebugSpring(Matrix4x4 local2World,bool byConvID) {
			if (springs!=null)
			for (int t=0;t<springs.Count;t++) {
				springs[t].DebugSpring(local2World,(byConvID) ? convIDColor[springs[t].convergenceGroupID] : _Settings.springDebugColor);
			}
		}
		
		public void DebugAngles(Matrix4x4 local2World,bool byConvID) {
			if (angles!=null)
			for (int i=0;i<angles.Count;i++) {
				angles[i].DebugDraw(local2World, (byConvID) ? convIDColor[angles[i].convergenceGroupID] : _Settings.angleDebugColor);
			}
		}
		
		//max 9 diff. id
		static Color[] convIDColor = new Color[] {Color.blue,Color.yellow,Color.red,Color.green,Color.cyan,Color.magenta,Color.white,Color.grey,Color.black};
	
		#endregion


	}
	
	

}