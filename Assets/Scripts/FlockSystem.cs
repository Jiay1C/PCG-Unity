using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlockSystem : MonoBehaviour
{
    public static FlockSystem Instance { get; private set; }
    
    public Vector3 minBound = new (-10, -10, -10);
    public Vector3 maxBound = new (10, 10, 10);
    public FlockConfig config;

    public bool followCameraPositionX;
    public bool followCameraPositionY;
    public bool followCameraPositionZ;
    public bool moveObjectRootPosition;

    public bool drawBoundary;
    public bool drawVelocity;
    
    private struct ObjectPersistentData
    {
        public Vector3 WanderForce;
    }
    
    private GameObject _rootObject;
    private List<(GameObject, Rigidbody)> _objects;
    private Dictionary<GameObject, ObjectPersistentData> _persistentData;
    private GameObject _template;

    public void Run()
    {
        if (_objects != null)
        {
            Reset();
            return;
        }
        
        _rootObject = new GameObject("Flock Objects");
        _objects = new List<(GameObject, Rigidbody)>((int)config.count);
        _persistentData = new Dictionary<GameObject, ObjectPersistentData>();
        
        InitTemplate();
        MatchObjectCount();
    }

    public void Stop()
    {
        _objects = null;
        
        _persistentData = null;

        if (_rootObject != null)
        {
            Destroy(_rootObject);
        }
        _rootObject = null;
        
        if (_template != null)
        {
            Destroy(_template);
        }
        _template = null;
    }

    public void Reset()
    {
        if (_objects != null)
        {
            foreach (var (obj, rb) in _objects)
            {
                ResetObject(obj, rb);
            }
        }
    }

    private void ResetObject(GameObject obj, Rigidbody rb)
    {
        obj.transform.localPosition = new Vector3(
            Random.Range(minBound.x, maxBound.x),
            Random.Range(minBound.y, maxBound.y),
            Random.Range(minBound.z, maxBound.z)
        );
        obj.transform.localRotation = Quaternion.Euler(
            Random.Range(0f, 360f),
            Random.Range(0f, 360f),
            Random.Range(0f, 360f)
        );
        rb.velocity = obj.transform.forward * Random.Range(config.minSpeed, config.maxSpeed);
        obj.GetComponent<TrailRenderer>().Clear();
    }

    private void InitTemplate()
    {
        _template = ModelSpawner.Instance.Spawn(config.type);
        _template.transform.parent = _rootObject.transform;
        
        Rigidbody rb = _template.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.angularDrag = config.angularDrag;

        TrailRenderer trail = _template.AddComponent<TrailRenderer>();
        trail.material = config.trailMaterial;
        trail.time = config.trailTime;
        trail.startWidth = config.trailWidth;
        trail.endWidth = config.trailWidth;
        trail.enabled = config.enableTrail;
        
        SphereCollider sc = _template.AddComponent<SphereCollider>();
        
        _template.SetActive(false);
    }

    private (GameObject, Rigidbody) InitObject()
    {
        GameObject obj = Instantiate(_template, _rootObject.transform);
        obj.name = $"{config.type} {_objects.Count}";
        
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        
        ResetObject(obj, rb);
        obj.SetActive(true);
        
        _persistentData[obj] = new ObjectPersistentData();
        
        return (obj, rb);
    }

    private void MatchObjectCount()
    {
        while (_objects.Count > config.count)
        {
            var (obj, rb) = _objects.Last();
            _objects.RemoveAt(_objects.Count - 1);
            Destroy(obj);
        }
        while (_objects.Count < config.count)
        {
            _objects.Add(InitObject());
        }
    }

    private void UpdateTrail()
    {
        TrailRenderer trail = _template.GetComponent<TrailRenderer>();
        if (trail.enabled != config.enableTrail)
        {
            Debug.Log(config.enableTrail ? "Enable Flock Trail" : "Disable Flock Trail");
            trail.enabled = config.enableTrail;
            foreach (var (obj, rb) in _objects)
            {
                obj.GetComponent<TrailRenderer>().enabled = config.enableTrail;
            }
        }
    }

    private void Update()
    {
        if (followCameraPositionX || followCameraPositionY || followCameraPositionZ)
        {
            Vector3 pos = transform.position;
            Vector3 cameraPos = CameraController.Instance.CameraPosition;
            if (followCameraPositionX) pos.x = cameraPos.x;
            if (followCameraPositionY) pos.y = cameraPos.y;
            if (followCameraPositionZ) pos.z = cameraPos.z;
            transform.position = pos;
            if (moveObjectRootPosition)
            {
                _rootObject.transform.position = pos;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_objects == null)
        {
            return;
        }

        UpdateTrail();
        MatchObjectCount();
        
        foreach (var (obj, rb) in _objects)
        {
            TeleportObject(obj);
            
            UpdateObject(obj, rb);
        }
    }

    private void TeleportObject(GameObject obj)
    {
        Vector3 position = obj.transform.position;
        position = transform.InverseTransformPoint(position);
        Vector3 newPosition = position;
        Vector3 boundSize = maxBound - minBound;
        
        if (newPosition.x < minBound.x) newPosition.x += boundSize.x;
        if (newPosition.x > maxBound.x) newPosition.x -= boundSize.x;
        
        if (newPosition.y < minBound.y) newPosition.y += boundSize.y;
        if (newPosition.y > maxBound.y) newPosition.y -= boundSize.y;
        
        if (newPosition.z < minBound.z) newPosition.z += boundSize.z;
        if (newPosition.z > maxBound.z) newPosition.z -= boundSize.z;

        if (newPosition != position)
        {
            obj.transform.position = transform.TransformPoint(newPosition);
            obj.GetComponent<TrailRenderer>().Clear();
        }
    }

    private void UpdateObject(GameObject obj, Rigidbody rb)
    {
        var data = _persistentData[obj];
        Vector3 position = obj.transform.position;

        var nearbyObjects = _objects
            .OrderBy(other=> Vector3.Distance(position, other.Item1.transform.position))
            .Take((int)config.nearbyObjectCount)
            .ToList();
        
        Vector3 totalForce = Vector3.zero;
        
        if (config.enableWanderForce)
        {
            data.WanderForce += config.coefWanderForce * Random.Range(-1f, 1f) * obj.transform.right;
            data.WanderForce += config.coefWanderForce * Random.Range(-1f, 1f) * obj.transform.up;
            if (data.WanderForce.magnitude > config.maxWanderForce)
            {
                data.WanderForce.Normalize();
                data.WanderForce *= config.maxWanderForce;
            }
            totalForce += data.WanderForce;
        }

        if (config.enableFlockCenterForce)
        {
            Vector3 center = Vector3.zero;
            foreach (var (nearbyObj, nearbyRb) in nearbyObjects)
            {
                center += nearbyObj.transform.position;
            }
            center /= nearbyObjects.Count;
            Vector3 force = center - position;
            float forceValue = force.magnitude * config.coefFlockCenterForce;
            force.Normalize();
            if (forceValue > config.maxFlockCenterForce)
            {
                force *= config.maxFlockCenterForce;
            }
            else
            {
                force *= forceValue;
            }
            totalForce += force;
        }

        if (config.enableCollisionAvoidForce)
        {
            Vector3 force = Vector3.zero;
            foreach (var (nearbyObj, nearbyRb) in nearbyObjects)
            {
                Vector3 dist = position - nearbyObj.transform.position;
                float distValue = config.maxCollisionAvoidRadius - dist.magnitude;
                if (distValue > 0)
                {
                    distValue *= distValue * config.coefCollisionAvoidForce;
                    force += dist.normalized * distValue;
                }
            }
            if (force.magnitude > config.maxCollisionAvoidForce)
            {
                force = force.normalized * config.maxCollisionAvoidForce;
            }
            totalForce += force;
        }

        if (config.enableVelocityMatchForce)
        {
            Vector3 force = Vector3.zero;
            foreach (var (nearbyObj, nearbyRb) in nearbyObjects)
            {
                force += nearbyRb.velocity.normalized;
            }
            force /= nearbyObjects.Count;
            float forceValue = force.magnitude * config.coefVelocityMatchForce;
            force.Normalize();
            if (forceValue > config.maxVelocityMatchForce)
            {
                force *= config.maxVelocityMatchForce;
            }
            else
            {
                force *= forceValue;
            }
            totalForce += force;
        }

        if (config.enableEnvironmentForce)
        {
            if (Physics.Raycast(position, obj.transform.forward, out var hit))
            {
                if (hit.distance < config.maxEnvironmentDistance)
                {
                    float forceValue = (config.maxEnvironmentDistance - hit.distance) / config.maxEnvironmentDistance;
                    forceValue *= forceValue;
                    forceValue *= config.coefEnvironmentForce;
                    totalForce += forceValue * hit.normal;
                }
            }
        }

        if (totalForce.magnitude > config.maxForce)
        {
            totalForce = totalForce.normalized * config.maxForce;
        }
        rb.AddForce(totalForce, ForceMode.VelocityChange);
        
        rb.MoveRotation(Quaternion.LookRotation(rb.velocity, obj.transform.up));
        
        float velocityValue = Mathf.Clamp(rb.velocity.magnitude, config.minSpeed, config.maxSpeed);
        rb.velocity = rb.velocity.normalized * velocityValue;
        
        float angularVelocityValue = Mathf.Clamp(rb.angularVelocity.magnitude, 0, config.maxAngularSpeed);
        rb.angularVelocity = rb.angularVelocity.normalized * angularVelocityValue;
        
        _persistentData[obj] = data;
    }

    private void Start()
    {
        Run();
    }

    private void OnDisable()
    {
        Stop();
    }

    private void OnDrawGizmos()
    {
        if (drawBoundary)
        {
            Vector3 center = (minBound + maxBound) * 0.5f;
            Vector3 size = maxBound - minBound;
        
            Gizmos.color = Color.cyan;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawWireCube(center, size);
            Gizmos.matrix = Matrix4x4.identity;
        }

        if (drawVelocity && _objects != null)
        {
            foreach (var (obj, rb) in _objects)
            {
                Vector3 pos = obj.transform.position;
                Vector3 pos1 = pos + rb.velocity;
                Vector3 pos2 = pos + rb.angularVelocity;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(pos, pos1);
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(pos, pos2);
            }
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
}
