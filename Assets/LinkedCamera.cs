using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedCamera : MonoBehaviour {

	Vector3 rotation;
	// Use this for initialization
	void Start () {
		rotation = transform.rotation.eulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 rot = transform.rotation.eulerAngles;
		//rot.x = rotation.x;
		rot.z = rotation.z;
		Quaternion rot2 = transform.rotation;
		rot2.eulerAngles = rot;
		transform.rotation = rot2;
	}
}
