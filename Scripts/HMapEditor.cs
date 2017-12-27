using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HMapEditor : MonoBehaviour {

	public int nwidth;
	public int nheight;
	public float nsize;

	// Use this for initialization
	void Start () {
		NewMap(nwidth, nheight, nsize, null);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// void NewMap(int width, int height, float size, Texture2D textures) {

	// }

	void NewMap(int width, int height, float size, Material textureMaterial) {
		GameObject mapPlane = new GameObject("Map");
		MeshRenderer mr = mapPlane.AddComponent<MeshRenderer>();
		MeshFilter mf = mapPlane.AddComponent<MeshFilter>();

		int numVertices = (width + 1) * (height + 1); // 4 vertices per tile, but edges are shared // 4
		int numTriangles = width * height * 6; // two triangles per tile // 6

		Mesh mesh = new Mesh();
		Vector3[] vertices = new Vector3[numVertices];
		int[] triangles = new int[numTriangles];


		for (int i = 0; i < width + 1; i++) {
			for (int j = 0; j < height + 1; j++) {
				int index = i + (j*(width + 1));
				vertices[index] = new Vector3(i * size, 0f, j * size);
			}
		}

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				int index = i + (j*width);
				int tripos = i + (j * (width+1));
				triangles[(index * 6) + 0] = tripos;
				triangles[(index * 6) + 1] = tripos + (width + 1);
				triangles[(index * 6) + 2] = tripos + 1;
				triangles[(index * 6) + 3] = tripos + 1;
				triangles[(index * 6) + 4] = tripos + (width + 1);
				triangles[(index * 6) + 5] = tripos + 1 + (width + 1);
			}
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;

		mf.mesh = mesh;
		mr.material = textureMaterial;
	}
}
