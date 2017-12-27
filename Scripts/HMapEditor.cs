using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HMapEditor : MonoBehaviour {

	public int nwidth;
	public int nheight;
	public float nsize;

	public int xmod;
	public int ymod;
	public float newheight;

	public Material textureMaterial;

	List<List<GameObject>> tiles;

	// Use this for initialization
	void Start () {
		NewMap(nwidth, nheight, nsize, textureMaterial);
		SetVertexHeight(xmod, ymod, newheight);
		FlipTriangles(0, 0);
		FlipTriangles(1, 1);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// void NewMap(int width, int height, float size, Texture2D textures) {

	// }

	void NewMap(int width, int height, float size, Material textureMaterial) {
		tiles = new List<List<GameObject>>();

		GameObject mapPlane = new GameObject("Map");

		for (int i = 0; i < width; i++) {
			tiles.Add(new List<GameObject>());
			for (int j = 0; j < height; j++) {
				GameObject newTile = CreateTile(new Vector3(i * size, 0f, j * size), size, textureMaterial, mapPlane.transform);
				tiles[i].Add(newTile);
			}
		}
	}

	GameObject CreateTile(Vector3 position, float size, Material textureMaterial, Transform parent) {
		GameObject newTile = new GameObject("Tile");
		MeshRenderer meshRenderer = newTile.AddComponent<MeshRenderer>();
		MeshFilter meshFilter = newTile.AddComponent<MeshFilter>();

		Mesh mesh = new Mesh();
		Vector3[] vertices = new Vector3[4] { 
			new Vector3(0f, 0f, 0f),
			new Vector3(size, 0f, 0f),
			new Vector3(0f, 0f, size),
			new Vector3(size, 0f, size)
		};
		int[] triangles = new int[6] {
			0, 2, 1, 1, 2, 3
		};
		Vector2[] uv = new Vector2[4] {
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;

		newTile.transform.parent = parent;
		newTile.transform.localPosition = position;

		meshFilter.mesh = mesh;
		if (textureMaterial != null) meshRenderer.material = textureMaterial;
		mesh.RecalculateNormals();
		return newTile;
	}

	void SetVertexHeight(int x, int y, float newHeight) {
		if (x < tiles.Count && y < tiles[0].Count) 	{ SetTileVertexHeight(tiles[x]	[y],	0, newHeight); }
		if (x > 0 			&& y < tiles[0].Count) 	{ SetTileVertexHeight(tiles[x-1][y],	1, newHeight); }
		if (x < tiles.Count && y > 0) 				{ SetTileVertexHeight(tiles[x]	[y-1], 	2, newHeight); }
		if (x > 0 			&& y > 0) 				{ SetTileVertexHeight(tiles[x-1][y-1], 	3, newHeight); }
	}

	void SetTileVertexHeight(GameObject tile, int vertex, float newHeight) {
		Mesh mesh = tile.GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		vertices[vertex].y = newheight;
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
	}

	void FlipTriangles(int x, int y) {
		GameObject tile = tiles[x][y];
		Mesh mesh = tile.GetComponent<MeshFilter>().mesh;
		int[] triangles = mesh.triangles;
		if (triangles[2] == 1) {
			triangles = new int[6] { 0, 2, 3, 0, 3, 1 };
		} else {
			triangles = new int[6] { 0, 2, 1, 1, 2, 3 };
		}
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
	}
}
