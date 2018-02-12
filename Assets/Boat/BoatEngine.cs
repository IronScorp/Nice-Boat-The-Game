using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatEngine : MonoBehaviour {

	public OceanBase ocean;
	// Use this for initialization
	void Start () {
		
	}

	public float power = 1F;
	public float acceleration = 1F;
	public float mobility = 1F;

	// Update is called once per frame
	void Update () {
		float v = Input.GetAxis ("Vertical"), next_speed = 0;
		if (v != 0) {
			next_speed += v * power;
		}

		float h = -Input.GetAxis ("Horizontal");
		Quaternion rotation = transform.localRotation;

		float y = rotation.eulerAngles.y;
		if (y > 180) y -= 360;

		if (h != 0) {
			if (Mathf.Abs (y) > 30)
				h = 0;
			else
				h *= mobility;
		} else {
			if (Mathf.Abs (y) < 1)
				h = -y ;
			else
				h = -y / 50;
		}
		
		rotation.eulerAngles += new Vector3(0, h, 0);
		transform.localRotation = rotation;
		h = rotation.eulerAngles.y * Mathf.Deg2Rad;

		Vector3 force = new Vector3 (Mathf.Sin(h), 0, Mathf.Cos(h));

		Vector3[] info = ocean.GetWaveInfo (
			transform.position.x, transform.position.z
		);
		float curPos = transform.position.y, dstPos = info [0].y;

		float distance = dstPos - curPos;
		float height = transform.lossyScale.x / 2;
		if (distance > -height) {
			force *= Mathf.Max (Mathf.Abs (distance / height), 1);
			force = transform.parent.rotation * force * next_speed;
			Rigidbody center = transform.parent.gameObject.GetComponent<Rigidbody> ();
			center.AddForceAtPosition (force, transform.position);
		}
	}


	float sgn(float x){  return (x > 0 ? 1 : -1);  }

	float abs(float x){  return Mathf.Abs(x);  }
}
