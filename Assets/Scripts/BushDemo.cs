using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BushDemo : MonoBehaviour
{
    public int randomSeed;
    public int bushCount;
    public Material bushMaterial;
    public Material leafMaterial;
    public Color[] bushColor;
    public Vector3[] bushPosition;
    
    void Start()
    {
        for (int i = 0; i < bushCount; i++)
        {
            (Mesh branchMesh, Mesh leafMesh) = MeshGenerator.Instance.Bush(randomSeed, i, i);
            
            GameObject bush = new GameObject($"Bush {i}", typeof(MeshFilter), typeof(MeshRenderer));
            bush.transform.parent = transform;
            bush.transform.position = bushPosition[i];
            
            Material material = new Material(bushMaterial);
            material.color = bushColor[i];
            bush.GetComponent<MeshRenderer>().material = material;
            
            bush.GetComponent<MeshFilter>().mesh = branchMesh;
            
            
            GameObject leaf = new GameObject($"Bush Leaf {i}", typeof(MeshFilter), typeof(MeshRenderer));
            leaf.transform.parent = bush.transform;
            leaf.transform.localPosition = Vector3.zero;
            
            leaf.GetComponent<MeshRenderer>().material = leafMaterial;
            
            leaf.GetComponent<MeshFilter>().mesh = leafMesh;
        }
    }
    
}
