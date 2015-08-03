using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace ParticlePhysics2D {

	public enum IntegrationMedthod {RUNGE_KUTTA, MODIFIED_EULER, VERLET, GPUVERLET}

	public class Simulation  {

		protected static float DEFAULT_GRAVITY = 0f;
		protected static float DEFAULT_DRAG = 0.001f;
		
		List <Particle2D> particles;
		List <Spring2D> springs;
		List<AngleConstraint2D> angles;
		
		IIntegrator integrator;
		
		Vector2 gravity;
		float drag;
		public float damping = 0.99f;//used by verlet
		
		
		bool hasDeadParticles = false;
		
		public void setIntegrator( IntegrationMedthod integrator )
		{
			switch ( integrator )
			{
			case IntegrationMedthod.RUNGE_KUTTA:
				this.integrator = new RungeKuttaIntegrator( this ) as IIntegrator;
				break;
			case IntegrationMedthod.MODIFIED_EULER:
				this.integrator = new ModifiedEulerIntegrator( this ) as IIntegrator;
				break;
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
			integrator.step( t );
		}
		
		#endregion
		
		#region Particles
		public Particle2D makeParticle(  float x, float y)
		{
			Particle2D p = new Particle2D( );
			p.Position = new Vector2 (x,y);
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
		
		public Spring2D makeSpring( Particle2D a, Particle2D b, float ks)
		{
			float r = Vector2.Distance(a.Position,b.Position);
			Spring2D s = new Spring2D( a, b, ks, r );
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
			integrator = new RungeKuttaIntegrator( this ) as IIntegrator;
			particles = new List<Particle2D> ();
			springs = new List<Spring2D> ();
			angles = new List<AngleConstraint2D> ();
			gravity = new Vector2 (0f,g);
			drag = somedrag;
		}
		
		public Simulation( float gx, float gy, float somedrag )
		{
			integrator = new RungeKuttaIntegrator( this ) as IIntegrator;
			particles = new List<Particle2D> ();
			springs = new List<Spring2D> ();
			angles = new List<AngleConstraint2D> ();
			gravity = new Vector2 (gx,gy);
			drag = somedrag;
		}
		
		public Simulation()
		{
			integrator = new RungeKuttaIntegrator( this ) as IIntegrator;
			particles = new List<Particle2D> ();
			springs = new List<Spring2D> ();
			angles = new List<AngleConstraint2D> ();
			gravity = new Vector2 (0f,Simulation.DEFAULT_GRAVITY);
			drag = Simulation.DEFAULT_DRAG;
		}
		
		public Simulation (float g,IntegrationMedthod intergratorMethod) {
			setIntegrator(intergratorMethod);
			particles = new List<Particle2D> ();
			springs = new List<Spring2D> ();
			angles = new List<AngleConstraint2D> ();
			gravity = new Vector2 (0f,g);
			drag = Simulation.DEFAULT_DRAG;
		}
		
		public Simulation (IntegrationMedthod intergratorMethod) {
			setIntegrator(intergratorMethod);
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
					Particle2D p = particles[i];
					p.Force += gravity;
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
		
	}
	
	

}