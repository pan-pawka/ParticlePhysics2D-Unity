using UnityEngine;
using System.Collections;

public class EventSingleton : MonoBehaviour {

	public static event System.Action OnClicked;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.anyKeyDown) {
			if (OnClicked!=null) {
				OnClicked();
				Debug.Log("Event Fired");
			}
		}
	}
}
