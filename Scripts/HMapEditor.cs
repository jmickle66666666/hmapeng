using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HMapEditor : MonoBehaviour {

	public Texture2D texture;

	Material textureMaterial;
	Vector2 tileUvSize = new Vector2(1f, 1f);
	List<List<GameObject>> tiles;
	List<List<int>> rotations;
	int textureColumns;

	// Control stuff 

	int selectedIdX = -1;
	int selectedIdY = -1;
	int currentTexture = 0;
	List<string> tools = new List<string>() { "tile", "texture", "vertex" };
	string currentTool = "tile";
	UnityEngine.UI.Text toolText;

	// Use this for initialization
	void Start () {
		toolText = GameObject.Find("ToolText").GetComponent<UnityEngine.UI.Text>();
		UpdateToolText();
		SetupMaterial(texture, 4, 4);
		NewMap(8, 8, 1f, textureMaterial);
		SetTileHeight(0, 0, 1f);
		SetTileHeight(0, 1, 1f);
		SetTileHeight(0, 2, 1f);

		FlipTriangles(1, 3);

		SetTileRectTextureIndex(0, 0, 3, 5, 1);

		SetTileTextureAndRotation(0, 3, 8, 2);
		SetTileTextureIndex(0, 0, 5);
		SetTileTextureIndex(0, 1, 5);
		SetTileTextureIndex(0, 2, 5);
		SetTileTextureIndex(1, 1, 4);
		RotateTexture(1, 1, 3);
		SetTileTextureIndex(1, 0, 8);
		RotateTexture(1, 0, 3);
		SetTileTextureIndex(1, 2, 8);
		RotateTexture(1, 2, 3);
		SetTileTextureIndex(1, 3, 10);
		RotateTexture(1, 3, 3);

		SetTileTextureIndex(2, 4, 7);


		// Roads
		SetTileRectTextureIndex(3, 0, 1, 5, 6);
		SetTileRectTextureIndex(0, 5, 8, 1, 6);
		SetTileRectTextureIndex(6, 6, 1, 2, 6);

		// Big wall at the top
		SetTileRectTextureIndex(0, 6, 6, 2, 12);
		SetTileTextureAndRotation(1, 7, 0, 0);
		SetTileHeight(0, 7, 1f);
		SetTileHeight(1, 7, 1f);
		SetTileHeight(2, 7, 1f);
		SetTileHeight(3, 7, 1f);
		SetTileHeight(4, 7, 1f);
		SetVertexHeight(0, 8, 2f);
		SetVertexHeight(1, 8, 2f);
		SetVertexHeight(2, 8, 2f);
		SetVertexHeight(3, 8, 2f);
		SetVertexHeight(4, 8, 2f);
		FlipTriangles(4, 7);
		FlipTriangles(5, 6);
		SetTileTextureIndex(4, 7, 13);
		SetTileTextureIndex(5, 6, 13);
		RotateTexture(5, 7, 3);

		SetTileRectTextureIndex(4, 0, 4, 5, 1);
		SetTileRectTextureIndex(7, 6, 1, 2, 1);
		SetTileHeight(5, 0, -1f);
		SetTileHeight(6, 0, -1f);
		SetTileHeight(7, 0, -1f);
		SetTileHeight(5, 1, -1f);
		SetTileHeight(6, 1, -1f);
		SetTileHeight(7, 1, -1f);
		SetTileHeight(5, 2, -1f);
		SetTileHeight(6, 2, -1f);
		SetTileHeight(7, 2, -1f);
		//SetVertexHeight(8, 4, -1f);
		FlipTriangles(7, 4);
		FlipTriangles(4, 3);
		SetTileRectTextureIndexAndRotation(4, 0, 1, 3, 14, 3);

		SetTileTextureAndRotation(4, 3, 15, 3);
		SetTileTextureAndRotation(4, 1, 11, 0);
		SetTileTextureAndRotation(5, 3, 3, 0);
		SetTileTextureAndRotation(6, 3, 14, 0);
		SetTileTextureAndRotation(7, 3, 14, 0);

		SetTileRectTextureIndex(5, 0, 3, 3, 2);
	}

	// Control stuff

	void UpdateToolText() {
		toolText.text = "Current tool: "+currentTool;
	}

	public void SelectTile(int id) {
		UnselectAll();
		selectedIdX = id % tiles.Count;
		selectedIdY = id / tiles[0].Count;
	}

	public void UnselectAll() {
		selectedIdX = -1;
		selectedIdY = -1;
		for (int i = 0; i < tiles.Count; i++) {
			for (int j = 0; j < tiles[0].Count; j++) {
				tiles[i][j].GetComponent<TileSelector>().selected = false;
			}
		}
	}

	void Update () {
		if (selectedIdX != -1) {
			if (Input.GetKeyDown(KeyCode.C)) {
				UnselectAll();
			}

			if (Input.GetKeyDown(KeyCode.R)) {
				RotateTexture(selectedIdX, selectedIdY, Input.GetKey(KeyCode.LeftShift)?3:1);
			}

			if (Input.GetKeyDown(KeyCode.F)) {
				FlipTexture(selectedIdX, selectedIdY);
			}
		}

		if (Input.GetKeyDown(KeyCode.Q)) {
			currentTool = tools[(tools.IndexOf(currentTool)+1)%tools.Count];
			UpdateToolText();
		}

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			hit.collider.GetComponent<TileSelector>().Hover();
		}

		if (Input.GetMouseButtonDown(0)) {
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit)) {
				hit.collider.GetComponent<TileSelector>().Select();
			} else {
				UnselectAll();
			}
		}
	}

	// Setup

	void SetupMaterial(Texture2D texture, int tileColumns, int tileRows) {
		textureMaterial = new Material(Shader.Find("Unlit/ColorTexture"));
		textureMaterial.SetTexture("_MainTex", texture);
		tileUvSize.Set(1f / tileColumns, 1f / tileRows);
		textureColumns = tileColumns;
		// Update all uvs here
	}

	void NewMap(int width, int height, float size, Material textureMaterial) {
		tiles = new List<List<GameObject>>();
		rotations = new List<List<int>>();

		GameObject mapPlane = new GameObject("Map");

		for (int i = 0; i < width; i++) {
			tiles.Add(new List<GameObject>());
			rotations.Add(new List<int>());
			for (int j = 0; j < height; j++) {
				GameObject newTile = CreateTile(new Vector3((i-(width/2)) * size, 0f, (j-(height/2)) * size), size, textureMaterial, mapPlane.transform);
				tiles[i].Add(newTile);
				rotations[i].Add(0);
				TileSelector sel = newTile.AddComponent<TileSelector>();
				sel.id = (j*width)+i;
			}
		}

		mapPlane.transform.Translate(width/2, 0f, height/2);
	}

	GameObject CreateTile(Vector3 position, float size, Material textureMaterial, Transform parent) {
		GameObject newTile = new GameObject("Tile");
		MeshRenderer meshRenderer = newTile.AddComponent<MeshRenderer>();
		MeshFilter meshFilter = newTile.AddComponent<MeshFilter>();

		Mesh mesh = new Mesh();
		Vector3[] vertices = new Vector3[4] { 
			new Vector3(0f, 0f, 0f),
			new Vector3(size, 0f, 0f),
			new Vector3(size, 0f, size),
			new Vector3(0f, 0f, size)
		};
		int[] triangles = new int[6] {
			0, 3, 2, 0, 2, 1
		};
		Vector2[] uv = new Vector2[4] {
			new Vector2(0f, 0f),
			new Vector2(tileUvSize.x, 0f),
			new Vector2(tileUvSize.x, tileUvSize.y),
			new Vector2(0f, tileUvSize.y)
		};

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;

		newTile.transform.parent = parent;
		newTile.transform.localPosition = position;

		meshFilter.mesh = mesh;
		if (textureMaterial != null) meshRenderer.material = textureMaterial;
		mesh.RecalculateNormals();

		newTile.AddComponent<MeshCollider>().sharedMesh = mesh;

		return newTile;
	}

	// Tile modification

	void SetTileHeight(int x, int y, float newHeight) {
		SetVertexHeight(x, y, newHeight);
		SetVertexHeight(x+1, y, newHeight);
		SetVertexHeight(x, y+1, newHeight);
		SetVertexHeight(x+1, y+1, newHeight);
	}

	void SetVertexHeight(int x, int y, float newHeight) {
		if (x < tiles.Count && y < tiles[0].Count) 	{ SetTileVertexHeight(tiles[x]	[y],	0, newHeight); }
		if (x > 0 			&& y < tiles[0].Count) 	{ SetTileVertexHeight(tiles[x-1][y],	1, newHeight); }
		if (x < tiles.Count && y > 0) 				{ SetTileVertexHeight(tiles[x]	[y-1], 	3, newHeight); }
		if (x > 0 			&& y > 0) 				{ SetTileVertexHeight(tiles[x-1][y-1], 	2, newHeight); }
	}

	void SetTileVertexHeight(GameObject tile, int vertex, float newHeight) {
		Mesh mesh = tile.GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		vertices[vertex].y = newHeight;
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		tile.GetComponent<MeshCollider>().sharedMesh = mesh;
	}

	Mesh GetTileMesh(int x, int y) {
		GameObject tile = tiles[x][y];
		return tile.GetComponent<MeshFilter>().mesh;
	}

	void FlipTriangles(int x, int y) {
		Mesh mesh = GetTileMesh(x, y);
		int[] triangles = mesh.triangles;
		if (triangles[2] == 2) {
			triangles = new int[6] { 0, 3, 1, 1, 3, 2 };
		} else {
			triangles = new int[6] { 0, 3, 2, 0, 2, 1 };
		}
		mesh.triangles = triangles;
		UpdateTile(x, y);
	}

	void SetTileTextureIndex(int x, int y, int textureIndex) {
		Mesh mesh = GetTileMesh(x, y);
		Vector2 baseUV = new Vector2((textureIndex % textureColumns) * tileUvSize.x, (textureIndex / textureColumns) * tileUvSize.y);
		Vector2[] uv = new Vector2[4] {
			new Vector2(baseUV.x, baseUV.y), // 0.75, 0.25 
			new Vector2(baseUV.x + tileUvSize.x, baseUV.y), // 0.5, 0.25
			new Vector2(baseUV.x + tileUvSize.x, baseUV.y + tileUvSize.y), // 0.5, 0.5
			new Vector2(baseUV.x, baseUV.y + tileUvSize.y) // 0.75, 0.5
		};
		mesh.uv = uv;
		RotateTexture(x, y, rotations[x][y]);
	}

	void RotateTexture(int x, int y, int rotation) {
		Mesh mesh = GetTileMesh(x, y);
		Vector2[] uv = mesh.uv;
		Vector2[] newUV = new Vector2[4];
		for (int i = 0; i < 4; i++) {
			newUV[i] = uv[(i + rotation) % 4];
		}
		mesh.uv = newUV;
		rotations[x][y] += rotation;
	}

	void FlipTexture(int x, int y) {
		Mesh mesh = GetTileMesh(x, y);
		Vector2[] uv = mesh.uv;
		Vector2[] newUV = new Vector2[4];
		newUV[0] = uv[1];
		newUV[1] = uv[0];
		newUV[2] = uv[3];
		newUV[3] = uv[2];
		mesh.uv = newUV;
	}

	void UpdateTile(int x, int y) {
		GameObject tile = tiles[x][y];
		tile.GetComponent<MeshFilter>().mesh.RecalculateNormals();
		tile.GetComponent<MeshCollider>().sharedMesh = tile.GetComponent<MeshFilter>().mesh;
	}

	// Macros

	void SetTileTextureAndRotation(int x, int y, int textureIndex, int rotation) {
		SetTileTextureIndex(x, y, textureIndex);
		RotateTexture(x, y, rotation);
	}

	void SetTileRectTextureIndex(int x, int y, int width, int height, int textureIndex) {
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				SetTileTextureIndex(x + i, y + j, textureIndex);
			}
		}
	}

	void SetTileRectTextureIndexAndRotation(int x, int y, int width, int height, int textureIndex, int rotation) {
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				SetTileTextureAndRotation(x + i, y + j, textureIndex, rotation);
			}
		}
	}


}
