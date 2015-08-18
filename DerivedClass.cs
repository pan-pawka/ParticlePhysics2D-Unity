using UnityEngine;
using System.Collections;

public class DerivedClass : BaseClass {

	
	
	protected override void OnClick () {
		base.OnClick();
		Debug.Log("Derived class click");
	}
	
	
}
