using UnityEngine;
using System.Collections;

namespace ParticlePhysics2D {
	public interface IFormLayer {
		Simulation GetSimulation {get;}
		event System.Action OnResetForm;
		event System.Action OnClearForm;
		void ResetForm();
		void ClearForm();
	}
}