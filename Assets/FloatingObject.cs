using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObject : MonoBehaviour {

	public OceanBase ocean;

	public float horizontalDrag = 0.5F;
	public float verticalDrag = 2F;
	// Use this for initialization
	void Start(){
		OnValidate ();
	}

	void OnValidate () { 
		Rigidbody mainBody = GetComponent<Rigidbody> ();
		mainBody.drag = 0;

		foreach (FixedJoint joint in GetComponents<FixedJoint>()) {
			joint.connectedBody.mass = mainBody.mass;
			joint.connectedBody.drag = mainBody.drag;
			joint.connectedBody.angularDrag = mainBody.angularDrag;
			//joint.connectedBody.GetComponent<MeshRenderer> ().enabled = false;
		}
	}

	bool inAir = true;
	void Move(Rigidbody component){
		Vector3[] info = ocean.GetWaveInfo (
			                 component.transform.position.x, component.transform.position.z
		                 );

		float curPos = component.transform.position.y, dstPos = info [0].y;

		float distance = dstPos - curPos;
		float height = component.transform.lossyScale.y / 2;
		Vector3 s = new Vector3 ();

		component.AddForce (Physics.gravity);
		if (distance > -height) {
			if (inAir && component.velocity.y < 0){
				Vector3 vel = component.velocity;
				vel.y /= 2;
				component.velocity = vel;
			}
			component.AddForce (s = info [1] * ocean.density * Mathf.Pow(2, 1 + height + distance));

		}
			
		inAir = !(distance > -height);

		//ScreenLog.SetInfo ("Body part " + component.GetHashCode (), distance + " " + s + " " + component.velocity);

	}
	void Update () {
		Rigidbody mainBody = GetComponent<Rigidbody> ();
		FixedJoint[] joints = GetComponents<FixedJoint> ();
		if (joints.Length == 0)
			Move (mainBody);

		Vector3[] info = ocean.GetWaveInfo (
			mainBody.transform.position.x, mainBody.transform.position.z
		);

		float curPos = mainBody.transform.position.y, dstPos = info [0].y;

		if (Mathf.Abs (curPos - dstPos) < GetComponent<Renderer> ().bounds.extents.y) {
			ParticleSystem system = mainBody.GetComponent<ParticleSystem> ();
			system.Emit (Mathf.Max (1, (int) mainBody.velocity.magnitude) / 5);
		}

		foreach (FixedJoint joint in joints) {
			Move(joint.connectedBody);
		}

		Vector3 dragVector = -mainBody.velocity.normalized;
		float magnitude = mainBody.velocity.magnitude * Time.deltaTime;
		dragVector.y *= verticalDrag * magnitude;
		dragVector.x *= horizontalDrag * magnitude;
		dragVector.z *= horizontalDrag * magnitude;

		mainBody.velocity += dragVector;
	}
}
