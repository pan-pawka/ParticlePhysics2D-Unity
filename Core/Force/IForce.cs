namespace ParticlePhysics2D {
public interface IForce
{
	void turnOn();
	void turnOff();
	bool isOn();
	bool isOff();
	void apply();
}
}