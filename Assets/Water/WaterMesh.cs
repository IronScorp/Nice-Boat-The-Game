using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class WaterMesh : MonoBehaviour {

	private Mesh mesh;
	private float elapsed = 0;

	public int polygonSize = 8;
	public float polygonResizer = 1.2F;

	void Generate(){

		int quarterSize = polygonSize / 4;
		polygonSize = quarterSize * 4;

		List<Vector3> vertices = new List<Vector3> ();
		List<int> triangles = new List<int> ();

		float resizer = polygonResizer;

		float x, z, end = resizer, step = 0, iStep, jStep, l, b;
		int _size = polygonSize + 1;

		for (int i = 0; i <= quarterSize; i++) {
			step += end;
			end *= resizer;
		}
			
		step = 0.5F / (step + quarterSize);
		iStep = end;

		l = 0;
		for (int i = 0, k = 0; i < _size; i++) {
			jStep = end;
			b = 0;
			for (int j = 0; j < _size; j++, k++) {
				Vector3 point = new Vector3 (-0.5F + step * l, 0, -0.5F + step * b);
				vertices.Add(point);
				if (i > 0 && j > 0) {
					triangles.Add (k - _size - 1);
					triangles.Add (k);
					triangles.Add (k - 1);

					triangles.Add (k - _size - 1);
					triangles.Add (k - _size);
					triangles.Add (k);
				}
				if (j <= quarterSize)
					jStep /= resizer;
				else if (j >= 3 * quarterSize)
					jStep *= resizer;
				b += jStep;
			}
			if (i <= quarterSize)
				iStep /= resizer;
			else if (i >= 3 * quarterSize)
				iStep *= resizer;
			l += iStep;
		}
		Mesh mesh = new Mesh ();
		GetComponent<MeshFilter> ().mesh = mesh;
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals ();
	}

	void GenerateClassic(){

		List<Vector3> vertices = new List<Vector3> ();
		List<int> triangles = new List<int> ();

		float step = 1.0F / polygonSize, x, z;
		int _size = polygonSize + 1;
		for (int i = 0, k=0; i < _size; i++) {
			for (int j = 0; j < _size; j++, k++) {
				x = -0.5F + step * i;      z = -0.5F + step * j;
				Vector3 point = new Vector3 (x, 0, z);
				vertices.Add(point);
				if (i > 0 && j > 0) {
					triangles.Add (k - _size - 1);
					triangles.Add (k);
					triangles.Add (k - 1);

					triangles.Add (k - _size - 1);
					triangles.Add (k - _size);
					triangles.Add (k);
				}
			}
		}
		Mesh mesh = new Mesh ();
		GetComponent<MeshFilter> ().mesh = mesh;
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals ();
	}

	void Start(){
		Generate ();
	}

	void OnValidate(){
		polygonSize = Mathf.Min (200, polygonSize);
		Generate ();
	}

	void Update(){
		transform.position = new Vector3 (
			transform.position.x,
			0,
			transform.position.z
		);
		transform.localRotation = Quaternion.Inverse (transform.parent.rotation);
	}
}

/*
public class Spline
{
	private class Polynomial{
		public float a=0, b=0, c=0, d=0;
		public Polynomial(){}
		public Polynomial(float y1, float f1, float y2, float f2){
			a = 2*(y1-y2) + f1 + f2;
			b = 3*(y2-y1) - 2*f1 - f2;
			c = f1;
			d = y1;
		}
		public static Polynomial operator+(Polynomial a, Polynomial b){
			Polynomial p = new Polynomial ();
			p.a = a.a + b.a; p.b = a.b + b.b; p.c = a.c + b.c; p.d = a.d + b.d; 
			return p;
		}
		public static Polynomial operator-(Polynomial a, Polynomial b){
			Polynomial p = new Polynomial ();
			p.a = a.a - b.a; p.b = a.b - b.b; p.c = a.c - b.c; p.d = a.d - b.d; 
			return p;
		}
		public float calculate(float val){
			return a * val * val * val + b * val * val + c * val + d;
		}
		public Polynomial GetDerivative(){
			Polynomial poly = new Polynomial ();
			poly.d = c;
			poly.c = b * 2;
			poly.b = a * 3;
			poly.a = 0;
			return poly;
		}
	}
	public static int calculating_count = 0;
	struct SplitPolynomial{
		public Polynomial partX;
		public Polynomial partZ;
		public Polynomial normalX;
		public Polynomial normalZ;
		public SplitPolynomial(Vector3 startLoc, Vector3 startDir, Vector3 endLoc, Vector3 endDir){
			partX = new Polynomial (startLoc.x, startDir.x, endLoc.x, endDir.x);
			partZ = new Polynomial (startLoc.z, startDir.z, endLoc.z, endDir.z);
			normalX = partZ.GetDerivative();
			normalZ = new Polynomial() - partX.GetDerivative();
		}

		private float F(Vector3 point, float value, float dir, float count){
			Vector2 normal = new Vector2 (normalX.calculate (value), normalZ.calculate (value));
			normal.Normalize();
			return Mathf.Pow (partX.calculate (value) + dir*(count+value)*normal.x - point.x, 2)
				 + Mathf.Pow (partZ.calculate (value) + dir*(count+value)*normal.y - point.z, 2);
		}

		private const float G = 1.6180339887498948F;
		private const float k = 5;
		private static float z = 2.0F * Mathf.PI - Mathf.Acos (k * k / 100);
		private delegate float CalculateFunc(float val, Vector3 point);
		public float FindHeight(Vector3 point, float dir, float count, int size){
			float min = 0, max = 1, x=0;
			for (int j = 0; j < 6; j++) {
				x = (max - min) / G;
				float x1 = max - x, x2 = min + x, fx1 = F(point, x1, dir, count), fx2 = F(point, x2, dir, count);
				calculating_count++;
				//Debug.Log (x1 + " " + fx1 + " " + x2 + " " + fx2);
				if (fx1 > fx2) min = x1;
				else max = x2;
				if (j > 1 && Mathf.Min(fx1, fx2) > z) return float.NaN;
			}
			x = (min + max) / 2;
			float fmin = F (point, min, dir, count), fmax = F (point, max, dir, count), fx = F (point, x, dir, count);
			if (fmin < fx) 	x = min;
			if (fmax < fx)  x = max;
			fx = Mathf.Sqrt (Mathf.Min (fx, fmin, fmax))*k;
			if (fx > z) return float.NaN;
			if (x == 1) {
				Vector2 one = new Vector2 (partX.calculate(1), partZ.calculate(1));
				Vector2 two = one - new Vector2 (partX.calculate(0.99F), partZ.calculate(0.99F));
				one = new Vector2 (point.x, point.z) - one;
				if (Vector2.Angle(one, two) < 85) return float.NaN;
			} else if (x == 0){
				Vector2 one = new Vector2 (partX.calculate(0), partZ.calculate(0));
				Vector2 two = one - new Vector2 (partX.calculate(0.01F), partZ.calculate(0.01F));
				one = new Vector2 (point.x, point.z) - one;
				if (Vector2.Angle(one, two) < 85) return float.NaN;
			}
			return (0.01F * k + Mathf.Cos (fx) / k) * (size - count - x);
		}
	}
	List<SplitPolynomial> parts = new List<SplitPolynomial>();

	private Vector3 startLoc, startDir;
	public Spline(Vector3 location, Vector3 direction){
		startLoc = location;
		startDir = direction;
	}

	public void Add(Vector3 location, Vector3 direction){
		parts.Add (new SplitPolynomial (startLoc, startDir, location, direction));
		
		startLoc = location;
		startDir = direction;
	}

	public float FindHeight(Vector3 location){
		float max = float.NaN, cur, count = 0;
		foreach (SplitPolynomial poly in parts){
			if ((cur = poly.FindHeight (location,  1, count, parts.Count)) > max || float.IsNaN(max))
				max = cur;
			if ((cur = poly.FindHeight (location, -1, count, parts.Count)) > max || float.IsNaN(max))
				max = cur;
			count++;
		}
		return max;
	}
}*/