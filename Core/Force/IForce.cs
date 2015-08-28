namespace ParticlePhysics2D {
public interface IForce
{
	void turnOn();
	void turnOff();
	bool isOn();
	bool isOff();
	void apply();
	
	// you need to SetSimulation in Simualtion class's OnAfterDeserialize() callback. this is a hack
	void SetParticles(Simulation sim);
}
}