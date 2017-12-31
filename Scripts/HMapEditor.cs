using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HMapEditor : MonoBehaviour {

	public Texture2D texture;

	float tileSize;
	Material textureMaterial;
	Vector2 tileUvSize = new Vector2(1f, 1f);
	List<List<GameObject>> tiles;
	List<List<int>> rotations;
	List<List<int>> tileTextureIndexes;
	List<List<bool>> tileFlips;
	int textureColumns;
	int textureRows;

	GameObject vertexMarker;
	LineRenderer vertexLineRenderer;

	List<string> changeHistory;

	// Control stuff 

	int selectedIdX = -1;
	int selectedIdY = -1;
	int hightlightIdX = -1;
	int hightlightIdY = -1;
	public int currentTexture = 0;
	List<string> tools = new List<string>() { "tile", "texture", "vertex" };
	string currentTool = "tile";
	UnityEngine.UI.Text toolText;
	UnityEngine.UI.RawImage texturePalette;
	char saveSeparater = '|';

	// Vertex moving stuff

	float vertexMoveStartHeight = 0f;
	float vertexMoveStartDepth = 0f;
	int vertexX = -1;
	int vertexY = -1;
	float[] tileHeights;

	bool newSceneDialog = false;

	// Use this for initialization
	void Start () {
		toolText = GameObject.Find("ToolText").GetComponent<UnityEngine.UI.Text>();
		UpdateToolText();

		texturePalette = GameObject.Find("Textures").GetComponent<UnityEngine.UI.RawImage>();

		vertexMarker = new GameObject("VertexMarker");
		vertexLineRenderer = vertexMarker.AddComponent<LineRenderer>();
		vertexLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
		vertexLineRenderer.SetPosition(0,new Vector3(0f, -1f, 0f));
		vertexLineRenderer.SetPosition(1,new Vector3(0f, 1f, 0f));
		vertexLineRenderer.startColor = new Color(0.8f, 0.4f, 0.2f, 1.0f);
		vertexLineRenderer.endColor = new Color(0.8f, 0.4f, 0.2f, 1.0f);
		vertexLineRenderer.enabled = false;
		vertexLineRenderer.useWorldSpace = false;
		vertexLineRenderer.startWidth = 0.1f;
		vertexLineRenderer.endWidth = 0.1f;

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

	public void HighlightTile(int id) {
		hightlightIdX = id % tiles.Count;
		hightlightIdY = id / tiles[0].Count;
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

	string newWidth = "8";
	string newHeight = "8";
	string newTextureRows = "4";
	string newTextureColumns = "4";
	string newTexture = "tiles.png";
	bool oopsDialog = false;
	bool saveDialog = false;
	bool doneDialog = false;
	bool loadDialog = false;
	string savePath = "map.json";
	string loadPath = "";

	void OnGUI() {
		if (newSceneDialog) {
			GUI.Box(new Rect(20, 20, 200, 175), "New Scene");
			GUI.Label(new Rect(25, 45, 80, 20), "Width");
			GUI.Label(new Rect(25, 70, 80, 20), "Height");
			GUI.Label(new Rect(25, 95, 80, 20), "Texture");
			GUI.Label(new Rect(25, 120, 80, 20), "Tex Columns");
			GUI.Label(new Rect(25, 145, 80, 20), "Tex Rows");

			newWidth = GUI.TextField(new Rect(100, 45, 105, 20), newWidth);
			newHeight = GUI.TextField(new Rect(100, 70, 105, 20), newHeight);
			newTexture = GUI.TextField(new Rect(100, 95, 105, 20), newTexture);
			newTextureColumns = GUI.TextField(new Rect(100, 120, 105, 20), newTextureColumns);
			newTextureRows = GUI.TextField(new Rect(100, 145, 105, 20), newTextureRows);

			if (GUI.Button(new Rect(25, 170, 190, 20), "OK!")) {
				try {
					Texture2D tex = new Texture2D(1,1);
					tex.LoadImage(File.ReadAllBytes(newTexture));
					tex.filterMode = FilterMode.Point;
					texturePalette.texture = tex;
					GameObject.Destroy(GameObject.Find("Map"));
					SetupMaterial(tex, int.Parse(newTextureColumns), int.Parse(newTextureRows));
					NewMap(int.Parse(newWidth), int.Parse(newHeight), 1.0f, textureMaterial);
					newSceneDialog = false;
					changeHistory = new List<string>();
					AddChange("n", newWidth, newHeight, newTexture, newTextureColumns, newTextureRows);
				} catch {
					newSceneDialog = false;
					oopsDialog = true;
				}
			}
		}

		if (saveDialog) {
			GUI.Box(new Rect(20, 20, 200, 75), "Save the thing!");
			GUI.Label(new Rect(25, 45, 80, 20), "Filename");
			savePath = GUI.TextField(new Rect(100,45,80,20), savePath);
			if (GUI.Button(new Rect(25,70,190,20), "do it")) {
				SaveMap(savePath);
				saveDialog = false;
				doneDialog = true;
			}
		}

		if (loadDialog) {
			GUI.Box(new Rect(20, 20, 200, 75), "Load a thing!");
			GUI.Label(new Rect(25, 45, 80, 20), "Filename");
			loadPath = GUI.TextField(new Rect(100,45,80,20), loadPath);
			if (GUI.Button(new Rect(25,70,190,20), "do it")) {
				LoadMap(loadPath);
				loadDialog = false;
				doneDialog = true;
			}
		}

		if (oopsDialog) {
			GUI.Box(new Rect(20, 20, 300, 25), "Oops! something broke. does the file exist? :/");
		}

		if (doneDialog) {
			GUI.Box(new Rect(20, 20, 300, 25), "Cool! done :)");
		}
	}

	void Update () {

		if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftControl)) {
			saveDialog = true;
		}

		if (Input.GetKeyDown(KeyCode.L) && Input.GetKey(KeyCode.LeftControl)) {
			loadDialog = true;
		}

		if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftControl)) {
			ReloadTexture();
		}

		if (Input.GetKeyDown(KeyCode.N)) {
			newSceneDialog = !newSceneDialog;
		}

		if (Input.GetKeyDown(KeyCode.Escape)) {
			newSceneDialog = false;
			oopsDialog = false;
			saveDialog = false;
			doneDialog = false;
			loadDialog = false;
		}

		if (newSceneDialog || saveDialog) return;

		vertexLineRenderer.enabled = (currentTool == "vertex");

		if (currentTool == "texture" && Input.GetKey(KeyCode.E)) {
			texturePalette.enabled = true;
		} else {
			texturePalette.enabled = false;
		}

		if (hightlightIdX != -1) {
			if (currentTool == "texture") {

				if (Input.GetKeyDown(KeyCode.R)) {
					RotateTexture(hightlightIdX, hightlightIdY, Input.GetKey(KeyCode.LeftShift)?3:1);
					AddChange("r", hightlightIdX, hightlightIdY, Input.GetKey(KeyCode.LeftShift)?3:1);
				}

				if (Input.GetKeyDown(KeyCode.F)) {
					FlipTexture(hightlightIdX, hightlightIdY);
					AddChange("m", hightlightIdX, hightlightIdY);
				}

			}
		}

		if (selectedIdX != -1) {

			if (currentTool != "tile") UnselectAll();

			if (Input.GetKeyDown(KeyCode.C)) {
				UnselectAll();
			}

			if (currentTool == "tile") {
				if (Input.GetKeyDown(KeyCode.F)) {
					FlipTriangles(selectedIdX, selectedIdY);
					AddChange("f", selectedIdX, selectedIdY);
				}
			}
		}

		if (Input.GetKeyDown(KeyCode.Q)) {
			currentTool = tools[(tools.IndexOf(currentTool)+1)%tools.Count];
			UpdateToolText();
		}


		
		hightlightIdX = -1;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {

			if (currentTool == "tile" || currentTool == "texture") {
				hit.collider.GetComponent<TileSelector>().Hover();
				int id = hit.collider.GetComponent<TileSelector>().id;
				HighlightTile(id);
			}
			
			if (currentTool == "vertex") {
				//vertexMoveStartDepth = hit.distance;
				if (!Input.GetMouseButton(0)) {
					vertexX = (int) Mathf.Round(hit.point.x / tileSize);
					vertexY = (int) Mathf.Round(hit.point.z / tileSize);
					vertexMarker.transform.position = new Vector3(vertexX * tileSize, GetHeightAt(vertexX, vertexY), vertexY * tileSize);
				}
			}
		}

		if (Input.GetMouseButtonDown(1)) {
			if (currentTool == "texture" && Input.GetKey(KeyCode.LeftControl)) {
				currentTexture = GetTextureIndex(hightlightIdX, hightlightIdY);
			}
		}

		if (Input.GetMouseButtonDown(0)) {

			if (currentTool == "vertex") {
				vertexMoveStartHeight = GetHeightAt(vertexX, vertexY);
				vertexMoveStartDepth = Camera.main.ScreenToViewportPoint(Input.mousePosition).y;
			}

			if (currentTool == "tile") {

				ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out hit)) {
					hit.collider.GetComponent<TileSelector>().Select();
				} else {
					UnselectAll();
				}

				if (selectedIdX != -1) {
					vertexMoveStartDepth = Camera.main.ScreenToViewportPoint(Input.mousePosition).y;
					tileHeights = new float[4] {
						GetHeightAt(selectedIdX, selectedIdY),
						GetHeightAt(selectedIdX + 1, selectedIdY),
						GetHeightAt(selectedIdX, selectedIdY + 1),
						GetHeightAt(selectedIdX + 1, selectedIdY + 1)
					};
					vertexMoveStartHeight = tileHeights[0];
				}
			}

			
		}

		if (Input.GetMouseButton(0)) {
			if (currentTool == "vertex") {
				Vector3 mouse = Camera.main.ScreenToViewportPoint(Input.mousePosition);
				float newHeight = ((mouse.y-vertexMoveStartDepth) * 10f) + vertexMoveStartHeight;
				newHeight = Mathf.Round(newHeight / 0.5f) * 0.5f;
				vertexMarker.transform.position = new Vector3(vertexMarker.transform.position.x, newHeight, vertexMarker.transform.position.z);

				SetVertexHeight(vertexX, vertexY, newHeight);
				AddChange("h", vertexX, vertexY, newHeight);
			}

			if (currentTool == "texture") {
				if (!Input.GetKey(KeyCode.E)) {
					if (hightlightIdX != -1) {
						if (GetTextureIndex(hightlightIdX,hightlightIdY) != currentTexture) {
							SetTileTextureIndex(hightlightIdX, hightlightIdY, currentTexture);
							AddChange("t", hightlightIdX, hightlightIdY, currentTexture);
						}
					}
				} else {
					
					if (Input.mousePosition.x < 300f && Input.mousePosition.y > Screen.height-300f) {
						float yp = (Screen.height - Input.mousePosition.y) * -1;
						int x = (int) ((Input.mousePosition.x / 300f) / tileUvSize.x);
						int y = (int) ((yp / 300f) / tileUvSize.y);
						currentTexture = x + ((y+textureRows - 1) * textureColumns);
					}
				}
			}

			if (currentTool == "tile") {
				if (selectedIdX != -1) {
					Vector3 mouse = Camera.main.ScreenToViewportPoint(Input.mousePosition);
					float hdiff = ((mouse.y-vertexMoveStartDepth) * 10f);
					hdiff = Mathf.Round(hdiff / 0.5f) * 0.5f;
					SetVertexHeight(selectedIdX, selectedIdY, hdiff + tileHeights[0]);
					SetVertexHeight(selectedIdX + 1, selectedIdY, hdiff + tileHeights[1]);
					SetVertexHeight(selectedIdX, selectedIdY + 1, hdiff + tileHeights[2]);
					SetVertexHeight(selectedIdX + 1, selectedIdY + 1, hdiff + tileHeights[3]);
					AddChange("h",selectedIdX, selectedIdY, hdiff + tileHeights[0]);
					AddChange("h",selectedIdX + 1, selectedIdY, hdiff + tileHeights[1]);
					AddChange("h",selectedIdX, selectedIdY + 1, hdiff + tileHeights[2]);
					AddChange("h",selectedIdX + 1, selectedIdY + 1, hdiff + tileHeights[3]);
				}
			}
		}

	}

	// Setup

	void ReloadTexture() {
		Debug.Log("reload");
		Texture2D tex = new Texture2D(1,1);
		tex.LoadImage(File.ReadAllBytes(newTexture));
		tex.filterMode = FilterMode.Point;
		texturePalette.texture = tex;
		textureMaterial.SetTexture("_MainTex", tex);
		for (int i = 0; i < tiles.Count; i++) {
			for (int j = 0; j < tiles[0].Count; j++) {
				tiles[i][j].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", tex);
			}
		}
	}

	void SetupMaterial(Texture2D texture, int tileColumns, int tileRows) {
		textureMaterial = new Material(Shader.Find("Unlit/ColorTexture"));
		textureMaterial.SetTexture("_MainTex", texture);
		tileUvSize.Set(1f / tileColumns, 1f / tileRows);
		textureColumns = tileColumns;
		textureRows = tileRows;
	}

	void NewMap(int width, int height, float size, Material textureMaterial) {
		tiles = new List<List<GameObject>>();
		rotations = new List<List<int>>();
		tileTextureIndexes = new List<List<int>>();
		tileFlips = new List<List<bool>>();
		tileSize = size;

		GameObject mapPlane = new GameObject("Map");

		for (int i = 0; i < width; i++) {
			tiles.Add(new List<GameObject>());
			rotations.Add(new List<int>());
			tileTextureIndexes.Add(new List<int>());
			tileFlips.Add(new List<bool>());

			for (int j = 0; j < height; j++) {
				GameObject newTile = CreateTile(new Vector3((i-(width/2)) * size, 0f, (j-(height/2)) * size), size, textureMaterial, mapPlane.transform);
				tiles[i].Add(newTile);
				rotations[i].Add(0);
				tileFlips[i].Add(false);
				tileTextureIndexes[i].Add(0);
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

	void SetTileHeight(int x, int y, float newHeight) { // not done
		SetVertexHeight(x, y, newHeight);
		SetVertexHeight(x+1, y, newHeight);
		SetVertexHeight(x, y+1, newHeight);
		SetVertexHeight(x+1, y+1, newHeight);

	}

	void ChangeTileHeight(int x, int y, float heightDiff) { // not done
		ChangeVertexHeight(x, y, heightDiff);
		ChangeVertexHeight(x+1, y, heightDiff);
		ChangeVertexHeight(x, y+1, heightDiff);
		ChangeVertexHeight(x+1, y+1, heightDiff);
	}

	float GetHeightAt(int x, int y) { // not needed
		if (x < tiles.Count && y < tiles.Count) { return GetVertexHeight(x, y, 0); }
		else if (x == tiles.Count && y < tiles.Count) { return GetVertexHeight(x-1, y, 1); }
		else if (x < tiles.Count && y == tiles.Count) { return GetVertexHeight(x, y-1, 3); }
		else  { return GetVertexHeight(x-1, y-1, 2); }
	}

	void ChangeVertexHeight(int x, int y, float heightDiff) { // not done
		SetVertexHeight(x, y, GetHeightAt(x, y) + heightDiff);
	}

	float GetVertexHeight(int x, int y, int vertex) { // not needed
		return GetTileMesh(x, y).vertices[vertex].y;
	}

	int GetTextureIndex(int x, int y) { // not needed
		return tileTextureIndexes[x][y];
	}

	void SetVertexHeight(int x, int y, float newHeight) { // not done
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

	Mesh GetTileMesh(int x, int y) { // not needed
		GameObject tile = tiles[x][y];
		return tile.GetComponent<MeshFilter>().mesh;
	}

	void FlipTriangles(int x, int y) { // not done
		Mesh mesh = GetTileMesh(x, y);
		tileFlips[x][y] = !tileFlips[x][y];
		int[] triangles = mesh.triangles;
		if (triangles[2] == 2) {
			triangles = new int[6] { 0, 3, 1, 1, 3, 2 };
		} else {
			triangles = new int[6] { 0, 3, 2, 0, 2, 1 };
		}
		mesh.triangles = triangles;
		UpdateTile(x, y);
	}

	void SetTileTextureIndex(int x, int y, int textureIndex) { // not done
		tileTextureIndexes[x][y] = textureIndex;
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

	void RotateTexture(int x, int y, int rotation) { // not done
		Mesh mesh = GetTileMesh(x, y);
		Vector2[] uv = mesh.uv;
		Vector2[] newUV = new Vector2[4];
		for (int i = 0; i < 4; i++) {
			newUV[i] = uv[(i + rotation) % 4];
		}
		mesh.uv = newUV;
		rotations[x][y] += rotation;
	}

	void FlipTexture(int x, int y) { // not done
		Mesh mesh = GetTileMesh(x, y);
		Vector2[] uv = mesh.uv;
		Vector2[] newUV = new Vector2[4];
		newUV[0] = uv[1];
		newUV[1] = uv[0];
		newUV[2] = uv[3];
		newUV[3] = uv[2];
		mesh.uv = newUV;
	}

	void UpdateTile(int x, int y) { // not needed
		GameObject tile = tiles[x][y];
		tile.GetComponent<MeshFilter>().mesh.RecalculateNormals();
		tile.GetComponent<MeshCollider>().sharedMesh = tile.GetComponent<MeshFilter>().mesh;
	}

	// Macros

	void AddChange(params object[] list) {
		string output = "";
		for (int i = 0; i < list.Length; i++) {
			output += list[i].ToString();
			if (i < list.Length-1) output += " ";
		}
		changeHistory.Add(output);
	}

	void SetTileTextureAndRotation(int x, int y, int textureIndex, int rotation) {// not done
		SetTileTextureIndex(x, y, textureIndex);
		RotateTexture(x, y, rotation);
	}

	void SetTileRectTextureIndex(int x, int y, int width, int height, int textureIndex) {// not done
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				SetTileTextureIndex(x + i, y + j, textureIndex);
			}
		}
	}

	void SetTileRectTextureIndexAndRotation(int x, int y, int width, int height, int textureIndex, int rotation) {// not done
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				SetTileTextureAndRotation(x + i, y + j, textureIndex, rotation);
			}
		}
	}

	void SaveMap(string path) {
		string output = "";
		for (int i = 0; i < changeHistory.Count; i++) {
			output += changeHistory[i];
			output += saveSeparater;
		}
		File.WriteAllText(path, output);
	}

	void LoadMap(string path) {
		string input = File.ReadAllText(path);
		string[] commands = input.Split(saveSeparater);
		changeHistory = new List<string>();

		for (int i = 0; i < commands.Length; i++) {
			AddChange(commands[i]);
			string[] command = commands[i].Split(' ');
			if (command[0] == "n") {
				Texture2D tex = new Texture2D(1,1);
				tex.LoadImage(File.ReadAllBytes(command[3]));
				tex.filterMode = FilterMode.Point;
				texturePalette.texture = tex;
				GameObject.Destroy(GameObject.Find("Map"));
				SetupMaterial(tex, int.Parse(command[4]), int.Parse(command[5]));
				NewMap(int.Parse(command[1]), int.Parse(command[2]), 1.0f, textureMaterial);
			}

			if (command[0] == "f") {
				FlipTriangles(int.Parse(command[1]),int.Parse(command[2]));
			}

			if (command[0] == "r") {
				RotateTexture(int.Parse(command[1]),int.Parse(command[2]),int.Parse(command[3]));
			}

			if (command[0] == "h") {
				SetVertexHeight(int.Parse(command[1]),int.Parse(command[2]),float.Parse(command[3]));
			}

			if (command[0] == "m") {
				FlipTexture(int.Parse(command[1]),int.Parse(command[2]));
			}

			if (command[0] == "t") {
				SetTileTextureIndex(int.Parse(command[1]),int.Parse(command[2]),int.Parse(command[3]));
			}
		}
   	}


}