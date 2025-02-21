using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterDemo : MonoBehaviour
{
    public int seed;
    public int count;
    public Vector3[] position;
    
    void Start()
    {
        Random.InitState(seed);
        for (int i = 0; i < count; i++)
        {
            GameObject helicopter = ModelSpawner.Instance.Helicopter(Random.Range(0, int.MaxValue));
            helicopter.name = $"Helicopter {i}";
            helicopter.transform.position = position[i];
            helicopter.transform.parent = transform;
        }
    }
}
