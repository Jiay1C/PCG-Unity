using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HelicopterSystem : MonoBehaviour
{
    public static HelicopterSystem Instance { get; private set; }
    
    public float speed = 30f;
    public int maxCount = 8;
    public float maxDistance = 50f;
    public float minSpawnHeight = 8f;
    public float maxSpawnHeight = 16f;
    public float updateFrequency = 1f;
    public float spawnMaxDelay = 5f;

    private GameObject[] _helicopters;

    private void Start()
    {
        _helicopters = new GameObject[maxCount];
        GameObject helicopterTemplate = ModelSpawner.Instance.Helicopter(Random.Range(0, int.MaxValue));
        helicopterTemplate.name = "Helicopter";
        helicopterTemplate.transform.parent = transform;
        helicopterTemplate.transform.position = new Vector3(0, -10 * maxDistance, 0);
        helicopterTemplate.AddComponent<Rigidbody>().useGravity = false;
        
        for (int i = 0; i < maxCount; i++)
        {
            _helicopters[i] = Instantiate(helicopterTemplate, transform);
            _helicopters[i].name = $"{helicopterTemplate.name} {i}";
        }
        
        Destroy(helicopterTemplate);
        
        StartCoroutine(DoUpdate());
    }
    
    private IEnumerator DoUpdate()
    {
        while (Application.isPlaying)
        {
            yield return new WaitForSeconds(updateFrequency);
            foreach (var helicopter in _helicopters)
            {
                Vector3 cameraPosition = CameraController.Instance.CameraPosition;
                if ((helicopter.transform.position - cameraPosition).magnitude >= maxDistance)
                {
                    Debug.Log($"Respawn {helicopter.name}");
                    helicopter.transform.position = cameraPosition + Vector3.up * Random.Range(minSpawnHeight, maxSpawnHeight);
                    helicopter.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    helicopter.GetComponent<Rigidbody>().velocity = helicopter.transform.right * speed;
                    yield return new WaitForSeconds(Random.Range(updateFrequency, spawnMaxDelay));
                }
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
