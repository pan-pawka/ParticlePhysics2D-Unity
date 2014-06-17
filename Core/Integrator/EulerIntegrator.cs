namespace ParticlePhysics2D {
public class EulerIntegrator : IIntegrator
{
	Simulation s;
	
	public EulerIntegrator( Simulation s )
	{
		this.s = s;
	}
	
	public void step( float t )
	{
		s.clearForces();
		s.applyForces();
		
		for ( int i = 0; i < s.numberOfParticles(); i++ )
		{
			Particle2D p = s.getParticle( i );
			if ( p.IsFree )
			{
				p.Velocity +=  p.Force/(p.Mass*t);
				p.Position += p.Velocity / t;
			}
		}
	}
	
}
}