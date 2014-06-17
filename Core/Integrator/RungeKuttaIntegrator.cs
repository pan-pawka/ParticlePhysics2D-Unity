using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace ParticlePhysics2D {
public class RungeKuttaIntegrator  {

	List<Vector2> originalPositions;
	List<Vector2> originalVelocities;
	List<Vector2> k1Forces;
	List<Vector2> k1Velocities;
	List<Vector2> k2Forces;
	List<Vector2> k2Velocities;
	List<Vector2> k3Forces;
	List<Vector2> k3Velocities;
	List<Vector2> k4Forces;
	List<Vector2> k4Velocities;
	
	Simulation s;
	
	public RungeKuttaIntegrator( Simulation s )
	{
		this.s = s;
		
		originalPositions = new List<Vector2> ();
		originalVelocities = new List<Vector2> ();
		k1Forces = new List<Vector2> ();
		k1Velocities = new List<Vector2> ();
		k2Forces = new List<Vector2> ();
		k2Velocities = new List<Vector2> ();
		k3Forces = new List<Vector2> ();
		k3Velocities = new List<Vector2> ();
		k4Forces = new List<Vector2> ();
		k4Velocities = new List<Vector2> ();
	}
	
	void allocateParticles()
	{
		while ( s.numberOfParticles() > originalPositions.Count) 
		{
			originalPositions.Add( Vector2.zero );
			originalVelocities.Add( Vector2.zero );
			k1Forces.Add( Vector2.zero );
			k1Velocities.Add( Vector2.zero );
			k2Forces.Add( Vector2.zero );
			k2Velocities.Add( Vector2.zero );
			k3Forces.Add( Vector2.zero);
			k3Velocities.Add( Vector2.zero );
			k4Forces.Add( Vector2.zero );
			k4Velocities.Add( Vector2.zero );
		}
	}
	
	public void step( float deltaT )
	{	
		allocateParticles();
		/////////////////////////////////////////////////////////
		// save original position and velocities
		
		for ( int i = 0; i < s.numberOfParticles(); ++i )
		{
			Particle2D p = s.getParticle(i);
			if ( p.IsFree )
			{
				Vector2 originalPosition = originalPositions[i];
				Vector2 k1Velocity = k1Velocities[i];
				
				p.Position = originalPosition + k1Velocity * 0.5f * deltaT;
				
				Vector2 originalVelocity = originalVelocities[i];
				Vector2 k1Force = k1Forces[i];
				
				p.Velocity = originalVelocity + k1Force * 0.5f * deltaT / p.Mass;
			}
		}
		
		s.applyForces();
		
		// save the intermediate forces
		for ( int i = 0; i < s.numberOfParticles(); ++i )
		{
			Particle2D p = s.getParticle(i);
			if ( p.IsFree )
			{
				k2Forces[i] = p.Force;
				k2Velocities[i] = p.Velocity;
			}
			
			p.Force = Vector2.zero;	// and clear the forces now that we are done with them
		}
		
		
		/////////////////////////////////////////////////////
		// get k3 values
		
		for ( int i = 0; i < s.numberOfParticles(); ++i )
		{
			Particle2D p = s.getParticle(i);
			if ( p.IsFree )
			{
				Vector2 originalPosition = originalPositions[i];
				Vector2 k2Velocity = k2Velocities[i];
				
				p.Position = originalPosition + k2Velocity * 0.5f * deltaT;
				
				Vector2 originalVelocity = originalVelocities[i];
				Vector2 k2Force = k2Forces[i];
				
				p.Velocity = originalVelocity + k2Force * 0.5f * deltaT / p.Mass;
			}
		}
		
		s.applyForces();
		
		// save the intermediate forces
		for ( int i = 0; i < s.numberOfParticles(); ++i )
		{
			Particle2D p = s.getParticle(i);
			if ( p.IsFree )
			{
				k3Forces[i] = p.Force;
				k3Velocities[i] = p.Velocity;
			}
			
			p.Force = Vector2.zero;	// and clear the forces now that we are done with them
		}
		
		
		//////////////////////////////////////////////////
		// get k4 values
		
			
		for ( int i = 0; i < s.numberOfParticles(); ++i )
		{
			Particle2D p = s.getParticle(i);
			if ( p.IsFree )
			{
				Vector2 originalPosition = originalPositions[i];
				Vector2 k3Velocity = k3Velocities[i];
				
				p.Position = originalPosition + k3Velocity * 0.5f * deltaT;
				
				Vector2 originalVelocity = originalVelocities[i];
				Vector2 k3Force = k3Forces[i];
				
				p.Velocity = originalVelocity + k3Force * 0.5f * deltaT / p.Mass;
				
			}
		}
		
		s.applyForces();
		
		// save the intermediate forces
		for ( int i = 0; i < s.numberOfParticles(); ++i )
		{
			Particle2D p = s.getParticle( i );
			if ( p.IsFree )
			{
				k4Forces[i] = p.Force;
				k4Velocities[i] = p.Velocity;
			}
		}
		
		/////////////////////////////////////////////////////////////
		// put them all together and what do you get?
		
		for ( int i = 0; i < s.numberOfParticles(); ++i )
		{
			Particle2D p = s.getParticle(i);
			p.Age += deltaT;
			if ( p.IsFree )
			{
				// update position
				
				Vector2 originalPosition = originalPositions[i];
				Vector2 k1Velocity = k1Velocities[i];
				Vector2 k2Velocity = k1Velocities[i];
				Vector2 k3Velocity = k3Velocities[i];
				Vector2 k4Velocity = k4Velocities[i];
				
				p.Position = originalPosition + deltaT / 6.0f * (k1Velocity + 2.0f * k2Velocity + 2.0f * k3Velocity + k4Velocity);
				// update velocity
				
				Vector2 originalVelocity = originalVelocities[i];
				
				Vector2 k1Force = k1Forces[i];
				Vector2 k2Force = k2Forces[i];
				Vector2 k3Force = k3Forces[i];
				Vector2 k4Force = k4Forces[i];
				
				p.Velocity = originalVelocity + deltaT / ( 6.0f * p.Mass) * (k1Force + 2.0f*k2Force + 2.0f*k3Force + k4Force);
			}
		}
	}
}
}
