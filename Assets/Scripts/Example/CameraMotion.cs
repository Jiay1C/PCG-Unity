// This sample code demonstrates how to create geometry "on demand" based on camera motion.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour {

	int max_plane = -1;       // the number of planes that we have made
	float plane_size = 3.0f;  // the size of the planes

	void Start () {
		
		// start with one plane
		create_new_plane();
	}
	
	// move the camera, and perhaps create a new plane
	void Update () {

		// get the horizontal and vertical controls (arrows, or WASD keys)
		float dx = Input.GetAxis ("Horizontal");
		float dz = Input.GetAxis ("Vertical");

		// sensitivity factors for translate and rotate
		float translate_factor = 0.3f;
		float rotate_factor = 5.0f;

		// move the camera based on keyboard input
		if (Camera.main != null) {
			// translate forward or backwards
			Camera.main.transform.Translate (0, 0, dz * translate_factor);

			// rotate left or right
			Camera.main.transform.Rotate (0, dx * rotate_factor, 0);

		}

		// get the main camera position
		Vector3 cam_pos = Camera.main.transform.position;
		//Debug.LogFormat ("x z: {0} {1}", cam_pos.x, cam_pos.z);

		// if the camera has moved far enough, create another plane
		if (cam_pos.z > (max_plane + 0.5) * plane_size * 2) {
			create_new_plane ();
		}

	}

	// create a new plane
	void create_new_plane() {

		int index = max_plane + 1;
		float plane_scale = plane_size / 5.0f;

		// make a new plane
		GameObject s = GameObject.CreatePrimitive(PrimitiveType.Plane);
		s.name = index.ToString("Plane 0");  // give this plane a name

		// modify the size and position of the plane
		s.transform.localScale = new Vector3 (plane_scale, plane_scale, plane_scale);
		// move plane to proper location in z
		s.transform.position = new Vector3 (0.0f, 0.0f, 2 * plane_size * (index + 0.0f));

		// change the plane's color
		Renderer rend = s.GetComponent<Renderer>();
		// alternate between two colors
		if (max_plane % 2 == 0)
			rend.material.color = new Color (0.2f, 0.2f, 0.7f, 1.0f);
		else
			rend.material.color = new Color (0.7f, 0.1f, 0.1f, 1.0f);
		
		// increment the number of planes that we've created
		max_plane++;
	}
}
