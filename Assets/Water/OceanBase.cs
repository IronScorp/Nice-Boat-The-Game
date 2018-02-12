using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OceanBase : MonoBehaviour {

	public float density = 1;
	public float resizer = 0.01F;

	public float[] countX;
	public float[] countZ;
	public float[] curveness;
	public float[] offset; 
	public float[] height;
	public float speed = 0.1F;
	public float allHeight = 0.1F;
	public float noiseScale = 0F;

	public int wavesCount(){
		return Mathf.Min (countX.Length, countZ.Length, curveness.Length, offset.Length, height.Length);
	}

	private float[] waves = new float[30];

	public Vector3[] GetWaveInfo(float x, float z){
		Vector3 vertex = new Vector3 (x, 0, z), normal = new Vector3 (0, 1/waves[2], 0);
		if (waves[0] == 0){
			return new Vector3[]{ vertex, normal };
		}
		float t = Time.time;

		float A = vertex.x * waves[2], B = vertex.z * waves[2];
		for (int i = 0; i < waves[0]; i++) {
			int ind = 5 + i*5;

			float C = A*waves [ind] + B * waves [ind+1] + t * waves[1] + waves [ind+3], s = Mathf.Sin(C), p = Mathf.Pow(Mathf.Abs(s),  waves [ind+2]), 
			n = waves[3] * waves [ind+4] * ( waves [ind+2] * p / s * Mathf.Cos(C) + Mathf.Sin (C*2) * 2 / waves [ind+2]);

			vertex.y += (Mathf.Abs(p) - Mathf.Cos(C*2) / waves [ind+2]) * waves [ind+4];
			normal.x -= waves [ind]   * n;
			normal.z -= waves [ind+1] * n;
		}
		vertex.y *= waves[3];
		normal.Normalize ();
		return new Vector3[]{ vertex, normal };
	}

	public Vector3 GetWaveSpeed(float x){
		Vector3 result = new Vector3 ();
		for (int i = 0; i < waves [0]; i++) 
			result += new Vector3 (1/countZ [i], 0, 1/countX [i]);

		result *= -speed;
		result.x /= transform.lossyScale.x;
		result.z /= transform.lossyScale.z;
		return result;
	}

	void Start(){
		UpdateMaterial ();
		OnValidate ();
	}

	private bool shouldUpdate = false;
	void OnValidate(){
		shouldUpdate = true;
	}

	public Material[] oceanBasedMaterials = new Material[0];
	void UpdateMaterial (){
		int c = wavesCount ();
		waves = new float[28];
		waves [0] = c;
		waves [1] = speed;
		waves [2] = resizer;
		waves [3] = allHeight;
		waves [4] = noiseScale;
		int i = 0;
		for (i = 0; i < c; i++) {
			waves [5 + i * 5] = countX [i];
			waves [6 + i * 5] = countZ [i];
			waves [7 + i * 5] = curveness [i];
			waves [8 + i * 5] = offset [i];
			waves [9 + i * 5] = height [i];
		}
		if (oceanBasedMaterials == null || oceanBasedMaterials.Length == 0)
			return;
		foreach(Material material in oceanBasedMaterials)
			material.SetFloatArray ("waves", waves);
		shouldUpdate = false;
	}

	// Update is called once per frame
	void Update () {
		if (shouldUpdate)
			UpdateMaterial ();
	}
}
