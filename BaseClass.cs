using UnityEngine;
using System.Collections;

public class BaseClass : MonoBehaviour {

	// Use this for initialization
	protected virtual void Start () {
		EventSingleton.OnClicked += OnClick;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	protected virtual void OnClick () {
		Debug.Log("Base class click");
	}
	
	void OnDestroy () {
		EventSingleton.OnClicked -= OnClick;
	}
}
