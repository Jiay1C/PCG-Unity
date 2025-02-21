// This sample code demonstrates how to create a texture using Perlin noise.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturedMesh : MonoBehaviour {

	public int texture_width = 64;
	public int texture_height = 64;
	public float scale = 10;

	// create a quad that is textured
	void Start () {

		// call the routine that makes a cube from scratch
		Mesh my_mesh = CreateMyMesh();

		// create a new GameObject and give it a MeshFilter and a MeshRenderer
		GameObject s = new GameObject("Textured Mesh");
		s.AddComponent<MeshFilter>();
		s.AddComponent<MeshRenderer>();

		// associate my_mesh with this object
		s.GetComponent<MeshFilter>().mesh = my_mesh;

		// change the color of the object
		Renderer rend = s.GetComponent<Renderer>();
		rend.material.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);

		// create a texture
		Texture2D texture = make_a_texture();

		// attach the texture to the mesh
		Renderer renderer = s.GetComponent<Renderer>();
		renderer.material.mainTexture = texture;
	}

	// create a texture with Perlin noise
	Texture2D make_a_texture() {

		// create the texture and an array of colors that will be copied into the texture
		Texture2D texture = new Texture2D (texture_width, texture_height);
		Color[] colors = new Color[texture_width * texture_height];

		// create the Perlin noise pattern in "colors"
		for (int i = 0; i < texture_width; i++)
			for (int j = 0; j < texture_height; j++) {
				float x = scale * i / (float) texture_width;
				float y = scale * j / (float) texture_height;
				float t = Mathf.PerlinNoise (x, y);                          // Perlin noise!
				colors [j * texture_width + i] = new Color (t, t, t, 1.0f);  // gray scale values (r = g = b)
			}

		// copy the colors into the texture
		texture.SetPixels(colors);

		// do texture specific stuff, probably including making the mipmap levels
		texture.Apply();

		// return the texture
		return (texture);
	}

	// create a mesh that consists of two triangles that makes up a quad
	Mesh CreateMyMesh() {
		
		// create a mesh object
		Mesh mesh = new Mesh();

		// the vertices of a cube
		Vector3[] verts = new Vector3[4];

		// vertices for a quad

		verts[0] = new Vector3 ( 1, 0, -1);
		verts[1] = new Vector3 ( 1, 0,  1);
		verts[2] = new Vector3 (-1, 0,  1);
		verts[3] = new Vector3 (-1, 0, -1);

		// create the uv coordinates

		Vector2[] uv = new Vector2[4];

		uv[0] = new Vector2(1, 0);
		uv[1] = new Vector2(1, 1);
		uv[2] = new Vector2(0, 1);
		uv[3] = new Vector2(0, 0);

		// two triangles for the face

		int[] tris = new int[6];  // need 3 vertices per triangle

		tris[0] = 0;
		tris[1] = 2;
		tris[2] = 1;
		tris[3] = 0;
		tris[4] = 3;
		tris[5] = 2;

		// save the vertices and triangles in the mesh object
		mesh.vertices = verts;
		mesh.triangles = tris;
		mesh.uv = uv;  // save the uv texture coordinates

		mesh.RecalculateNormals();  // automatically calculate the vertex normals

		return (mesh);
	}

	// update is called once per frame
	void Update () {

	}
		
}
