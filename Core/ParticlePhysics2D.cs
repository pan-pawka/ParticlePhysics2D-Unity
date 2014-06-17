using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace ParticlePhysics2D {

	public enum IntegrationMedthod {RUNGE_KUTTA, MODIFIED_EULER, VERLET}

	public class Simulation  {

		protected static float DEFAULT_GRAVITY = 0;
		protected static float DEFAULT_DRAG = 0.001f;	
		
		List <Particle2D> particles;
		List <Spring2D> springs;
		List <Attraction2D> attractions;
		List<IForce> customForces;
		
		IIntegrator integrator;
		
		Vector2 gravity;
		float drag;
		
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
			}
		}
		
		public void setGravity( float x, float y)
		{
			gravity = new Vector2 (x,y);
		}
		
		// default down gravity
		public void setGravity( float g )
		{
			gravity = new Vector2 (0f,g);
		}
		
		public void setDrag( float d )
		{
			drag = d;
		}
		
		public void tick()
		{
			tick( 1f );
		}
		
		public void tick( float t )
		{  
			integrator.step( t );
		}
		
		public Particle2D makeParticle( float mass, float x, float y, float z )
		{
			Particle2D p = new Particle2D( mass );
			p.Position = new Vector2 (x,y);
			particles.Add( p );
			return p;
		}
		
		public Particle2D makeParticle()
		{  
			return makeParticle( 1.0f, 0f, 0f, 0f );
		}
		
		public Spring2D makeSpring( Particle2D a, Particle2D b, float ks, float d, float r )
		{
			Spring2D s = new Spring2D( a, b, ks, d, r );
			springs.Add( s );
			return s;
		}
		
		public Attraction2D makeAttraction( Particle2D a, Particle2D b, float k, float minDistance )
		{
			Attraction2D m = new Attraction2D( a, b, k, minDistance );
			attractions.Add( m );
			return m;
		}
		
		public void clear()
		{
			particles.Clear();
			springs.Clear();
			attractions.Clear();
		}
		
		public Simulation( float g, float somedrag )
		{
			integrator = new RungeKuttaIntegrator( this ) as IIntegrator;
			particles = new List<Particle2D> ();
			springs = new List<Spring2D> ();
			attractions = new List<Attraction2D> ();
			customForces = new List<IForce> ();
			gravity = new Vector2 (0f,g);
			drag = somedrag;
		}
		
		public Simulation( float gx, float gy, float somedrag )
		{
			integrator = new RungeKuttaIntegrator( this ) as IIntegrator;
			particles = new List<Particle2D> ();
			springs = new List<Spring2D> ();
			attractions = new List<Attraction2D> ();
			customForces = new List<IForce> ();
			gravity = new Vector2 (gx,gy);
			drag = somedrag;
		}
		
		public Simulation()
		{
			integrator = new RungeKuttaIntegrator( this ) as IIntegrator;
			particles = new List<Particle2D> ();
			springs = new List<Spring2D> ();
			attractions = new List<Attraction2D> ();
			customForces = new List<IForce> ();
			gravity = new Vector2 (0f,Simulation.DEFAULT_GRAVITY);
			drag = Simulation.DEFAULT_DRAG;
		}
		
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
				particles[i].Force += - particles[i].Velocity * drag;
			}
			
			for ( int i = 0; i < springs.Count; i++ )
			{
				springs[i].apply();
			}
			
			for ( int i = 0; i < attractions.Count; i++ )
			{
				attractions[i].apply();
			}
			
			for ( int i = 0; i < customForces.Count; i++ )
			{
				customForces[i].apply();
			}
		}
		
		public void clearForces()
		{
			foreach (Particle2D p in particles) {
				p.Force = Vector2.zero;
			}
		}
		
		public int numberOfParticles()
		{
			return particles.Count;
		}
		
		public int numberOfSprings()
		{
			return springs.Count;
		}
		
		public int numberOfAttractions()
		{
			return attractions.Count;
		}
		
		public Particle2D getParticle( int i )
		{
			return particles[i];
		}
		
		public Spring2D getSpring( int i )
		{
			return springs[i];
		}
		
		public Attraction2D getAttraction( int i )
		{
			return attractions[i];
		}
		
		public void addCustomForce( IForce f )
		{
			customForces.Add( f );
		}
		
		public int numberOfCustomForces()
		{
			return customForces.Count;
		}
		
		public IForce getCustomForce( int i )
		{
			return customForces[i];
		}
		
		public void removeCustomForce( int i )
		{
			customForces.RemoveAt(i);
		}
		
		public void removeParticle( Particle2D p )
		{
			particles.Remove( p );
		}
		
		public void removeSpring( int i )
		{
			 springs.RemoveAt( i );
		}
		
		public void removeAttraction( int i  )
		{
			 attractions.RemoveAt( i );
		}
		
		public void removeAttraction( Attraction2D s )
		{
			attractions.Remove( s );
		}
		
		public void removeSpring( Spring2D a )
		{
			springs.Remove( a );
		}
		
		public void removeCustomForce( IForce f )
		{
			customForces.Remove( f );
		}
	}
	
	

}