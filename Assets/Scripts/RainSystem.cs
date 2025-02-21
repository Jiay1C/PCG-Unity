using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainSystem : MonoBehaviour
{
    public static RainSystem Instance { get; private set; }
    
    public Material rainMaterial;
    public int maxObjectNumber = 5000;
    public float initialHeight = 30f;
    public float rainDistance = 50f;
    public float rainQuantity = 500f;
    public float rainSize = 0.2f;
    public float updateFrequency = 10f;

    private float _objectNumberPerUpdate;
    private float _updateIntervalSeconds;
    private GameObject _rainRootObject;
    private Queue<GameObject> _rainQueue;

    public void StartRain()
    {
        if (_rainRootObject != null)
        {
            Destroy(_rainRootObject);
        }
        
        _updateIntervalSeconds = 1.0f / updateFrequency;
        _objectNumberPerUpdate = rainQuantity / updateFrequency;
        _rainRootObject = new GameObject("Rain Objects");
        _rainQueue = new Queue<GameObject>();
        for (int i = 0; i < maxObjectNumber; i++)
        {
            GameObject rainObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rainObject.name = $"Rain Object {i}";
            rainObject.transform.parent = _rainRootObject.transform;
            rainObject.transform.localScale = Vector3.one * rainSize;
            if (rainMaterial != null)
            {
                Renderer rainRenderer = rainObject.GetComponent<Renderer>();
                rainRenderer.material = rainMaterial;
            }
            Rigidbody rainRigidbody = rainObject.AddComponent<Rigidbody>();
            rainRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Vector2 pos = Random.insideUnitCircle * rainDistance;
            rainObject.transform.localPosition = new Vector3(pos.x, -100, pos.y);
            _rainQueue.Enqueue(rainObject);
        }
        
        StartCoroutine(DoRain());
    }

    public void StopRain()
    {
        StopAllCoroutines();
        _rainQueue.Clear();
        if (_rainRootObject != null)
        {
            Destroy(_rainRootObject);
            _rainRootObject = null;
        }
    }
    
    private IEnumerator DoRain()
    {
        while (Application.isPlaying)
        {
            for (int i = 0; i < _objectNumberPerUpdate; i++)
            {
                GameObject rainObject = _rainQueue.Dequeue();
                
                Vector2 pos = Random.insideUnitCircle * rainDistance;
                Vector3 cameraPos = CameraController.Instance.CameraPosition;
                rainObject.transform.position = new Vector3(pos.x + cameraPos.x, initialHeight, pos.y + cameraPos.z);
                rainObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                
                _rainQueue.Enqueue(rainObject);
            }
            yield return new WaitForSeconds(_updateIntervalSeconds);           
        }
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

    private void Start()
    {
        StartRain();
    }

    private void OnDisable()
    {
        StopRain();
    }
}
