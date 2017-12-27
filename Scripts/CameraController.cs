using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(1)) {
			transform.Rotate(-Input.GetAxis("Mouse Y") * 10f, Input.GetAxis("Mouse X") * 10f, 0f);
			transform.Translate(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
			transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f);
		}
	}
}
