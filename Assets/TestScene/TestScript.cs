using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var obj = new GameObject(); 
		var collider = obj.AddComponent<Light>();

		// Disable the collider (while the object is inactive) 
		obj.SetActive(false); 
		collider.enabled = false;

		// Destroying the object will result in "Assertion failed" errors 
		Destroy(obj);

	}
	
}
