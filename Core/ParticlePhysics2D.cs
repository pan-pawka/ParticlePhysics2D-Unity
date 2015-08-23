//Yves Wang @ FISH, 2015, All rights reserved
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


namespace ParticlePhysics2D {

	//public enum IntegrationMedthod {RUNGE_KUTTA, MODIFIED_EULER, VERLET, GPUVERLET}
	//RUNGE_KUTTA and MODIFIED_EULER intergrator support are dropped, but the relevant script are maintained
	//you can enable it by replacing the line below with the commented-out line above
	public enum IntegrationMedthod {VERLET, GPUVERLET}

	[System.Serializable]
	public class Simulation  : ISerializationCallbackReceiver {

		protected static float DEFAULT_GRAVITY = 0f;
		protected static float DEFAULT_DRAG = 0.001f;
		
		[HideInInspector] [SerializeField] List <Particle2D> particles;
		
		[HideInInspector] [SerializeField] List <Spring2D> springs;
		
		[HideInInspector] [SerializeField] List<AngleConstraint2D> angles;
		
		public IIntegrator integrator;
		
		[SerializeField]
		IntegrationMedthod integrationMedthod = IntegrationMedthod.VERLET;
		
		[SerializeField]
		Vector2 gravity;
		
		[SerializeField]
		float drag;
		
		[Range(0.01f,0.99f)]
		public float damping = 0.95f;//used by verlet
		
		[Range(0.005f,0.99f)]
		public float springConstant = 0.1f;//used by verlet
		
		
		//bool hasDeadParticles = false;
		
		public void setIntegrator()
		{
			switch ( integrationMedthod )
			{
//			case IntegrationMedthod.RUNGE_KUTTA:
//				this.integrator = new RungeKuttaIntegrator( this ) as IIntegrator;
//				break;
//			case IntegrationMedthod.MODIFIED_EULER:
//				this.integrator = new ModifiedEulerIntegrator( this ) as IIntegrator;
//				break;
			case IntegrationMedthod.VERLET:
				this.integrator = new VerletIntegrator( this ) as IIntegrator;
				break;
			case IntegrationMedthod.GPUVERLET:
				this.integrator = new GPUVerletIntegrator(this) as IIntegrator;
				break;
			default:
				return;
			}
		}
		
		public void setIntegrator(IntegrationMedthod integrator) 
		{
			integrationMedthod = integrator;
			setIntegrator();
		}
		
		
		public void setGravity( float x, float y)
		{
			gravity = new Vector2 (x,y);
		}
		
		// default gravity is down
		public void setGravity( float g )
		{
			gravity = new Vector2 (0f,g);
		}
		
		public void setDrag( float d )
		{
			drag = d;
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
		
		#region Serialization
		public void OnBeforeSerialize()  {}
		public void OnAfterDeserialize() {
			for (int i=0;i<springs.Count;i++) {
				springs[i].SetParticles(this);
			}
			this.setIntegrator(integrationMedthod);
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
		#endregion
		
		#region Tick
		
		/// <summary>
		/// advance the simulation by some time t, or by the default 1.0. 
		/// You'll want to call this in Update(). You probably want to keep this the same at all times 
		/// unless you want speed up or slow things down.
		/// </summary>
		public void tick()
		{
			tick( 1f );
		}
		
		public void tick( float t )
		{  
			//if (integrator==null) setIntegrator(integrationMedthod);
			integrator.step( t );
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
			
//			for (int i=angles.Count-1;i>=0;i--) {
//				if (angles[i].ParticleA == p || angles[i].ParticleM == p || angles[i].ParticleB == p) {
//					AngleConstraint2D a = angles[i];
//					removeAngleConstraint(a);
//				}
//			}
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
		
		#endregion
		
		#region Angle Constraints
		public AngleConstraint2D makeAngleConstraint( Spring2D s1, Spring2D s2, float offset, float relax)
		{
			AngleConstraint2D angle = new AngleConstraint2D (s1,s2,offset,relax);
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
		#endregion
		
		#region Simulation Constructor
		
		public Simulation( float g, float somedrag )
		{
			setIntegrator(integrationMedthod);
			particles = new List<Particle2D> ();
			springs = new List<Spring2D> ();
			angles = new List<AngleConstraint2D> ();
			gravity = new Vector2 (0f,g);
			drag = somedrag;
		}
		
		public Simulation( float gx, float gy, float somedrag )
		{
			setIntegrator(integrationMedthod);
			particles = new List<Particle2D> ();
			springs = new List<Spring2D> ();
			angles = new List<AngleConstraint2D> ();
			gravity = new Vector2 (gx,gy);
			drag = somedrag;
		}
		
		public Simulation()
		{
			setIntegrator(integrationMedthod);
			particles = new List<Particle2D> ();
			springs = new List<Spring2D> ();
			angles = new List<AngleConstraint2D> ();
			gravity = new Vector2 (0f,Simulation.DEFAULT_GRAVITY);
			drag = Simulation.DEFAULT_DRAG;
		}
		
		public Simulation (float g,IntegrationMedthod integrationMedthod) {
			setIntegrator(integrationMedthod);
			particles = new List<Particle2D> ();
			springs = new List<Spring2D> ();
			angles = new List<AngleConstraint2D> ();
			gravity = new Vector2 (0f,g);
			drag = Simulation.DEFAULT_DRAG;
		}
		
		public Simulation (IntegrationMedthod integrationMedthod) {
			setIntegrator(integrationMedthod);
			particles = new List<Particle2D> ();
			springs = new List<Spring2D> ();
			angles = new List<AngleConstraint2D> ();
			gravity = new Vector2 (0f,Simulation.DEFAULT_GRAVITY);
			drag = Simulation.DEFAULT_DRAG;
		}
		
		#endregion
		
		#region Called by Integrator
		/// <summary>
		/// ApplyForces is called by Integrator to advanced the system, note that GPU Verlet Intergrator does not call this
		/// </summary>
		public void applyForces()
		{
			if ( gravity != Vector2.zero )
			{
				for ( int i = 0; i < particles.Count; ++i )
				{
					particles[i].Force += gravity;
				}
			}
			
			for ( int i = 0; i < particles.Count; ++i )
			{
				particles[i].Force += - particles[i].Velocity * drag;//called by intergartors except for GPU Verlet one
			}
			
			for ( int i = 0; i < springs.Count; i++ )
			{
				springs[i].apply();
			}
			
			for ( int i = 0; i < angles.Count; i++ )
			{
				angles[i].apply();
			}
		}
		
		/// <summary>
		/// clearForces is called by Integrator to advanced the system
		/// </summary>
		public void clearForces()
		{
			for (int i=0;i<particles.Count;i++) {
				particles[i].Force = Vector2.zero;
			}
		}
		
		#endregion
		
		#region Debug
		public void DebugSpring(Matrix4x4 local2World) {
			for (int t=0;t<springs.Count;t++) {
				springs[t].DebugSpring(local2World);
			}
		}
		public void DebugSpring() {
			for (int t=0;t<springs.Count;t++) {
				springs[t].DebugSpring();
			}
		}
		
		//use this inside the editor script
		public void DebugParticle(Matrix4x4 local2World) {
			for (int i=0;i<particles.Count;i++) {
				Vector2 pos = local2World.MultiplyPoint3x4(particles[i].Position);
				DebugExtension.DebugPoint(pos,Color.blue,2f);
				Handles.Label(pos,new GUIContent(i.ToString()));
			}
		}
		
		public void DebugParticle() {
			for (int i=0;i<particles.Count;i++) {
				Vector2 pos = particles[i].Position;
				DebugExtension.DebugPoint(pos,Color.blue,2f);
				Handles.Label(pos,new GUIContent(i.ToString()));
			}
		}
		
		
		#endregion
		
	}
	
	

}