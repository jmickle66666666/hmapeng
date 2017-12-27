using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSelector : MonoBehaviour {

	HMapEditor maped;
	Material mat;
	public bool selected;
	public int id;

	// Use this for initialization
	void Start () {
		maped = GameObject.Find("HMapEditor").GetComponent<HMapEditor>();
		mat = GetComponent<MeshRenderer>().material;
	}
	
	// Update is called once per frame
	void Update () {
		if (!selected) mat.SetColor("_Color", new Color(0.0f, 0.0f, 0.0f, 1.0f));
	}

	public void Select() {
		maped.SelectTile(id);
		selected = true;
		mat.SetColor("_Color", new Color(0.2f, 0.4f, 0.0f, 1.0f));
	}

	public void Unselect() {
		selected = false;
	}

	public void Hover() {
		if (!selected) mat.SetColor("_Color", new Color(0.3f, 0.15f, 0.0f, 1.0f));
	}
}
