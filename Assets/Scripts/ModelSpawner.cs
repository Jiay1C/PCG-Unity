using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ModelSpawner : MonoBehaviour
{
    public static ModelSpawner Instance { get; private set; }
    
    public HelicopterConfig helicopterConfig;
    public DroneConfig droneConfig;

    private const string _disableRandomTag = "DisableRandom";
    private const string _leftWeaponRootName = "LeftWeapon";
    private const string _rightWeaponRootName = "RightWeapon";
    private const string _frontWeaponRootName = "FrontWeapon";

    public GameObject Spawn(string type, int seed = 0)
    {
        switch (type)
        {
            case "Drone":
                return Drone(seed);
            case "Helicopter":
                return Helicopter(seed);
            default:
                throw new ArgumentOutOfRangeException($"Unknown Type: {type}");
        }
    }

    public GameObject Drone(int seed = 0)
    {
        GameObject drone = Instantiate(droneConfig.prefab);
        drone.name = "Drone";
        drone.GetComponent<ImplicitSurfaceManager>().Polygonize();
        return drone;
    }

    public GameObject Helicopter(int seed = 0)
    {
        Random.State state = Random.state;
        Random.InitState(seed);
        
        GameObject helicopter = Instantiate(helicopterConfig.prefab);
        helicopter.name = "Helicopter";
        
        foreach (ImplicitSurfaceInfo implicitSurfaceInfo in helicopter.GetComponentsInChildren<ImplicitSurfaceInfo>())
        {
            Transform componentTransform = implicitSurfaceInfo.gameObject.transform;
            
            if (!componentTransform.parent.gameObject.name.Contains(_disableRandomTag))
            {
                float scaleX = Random.Range(helicopterConfig.scaleRange.x, helicopterConfig.scaleRange.y);
                float scaleY = Random.Range(helicopterConfig.scaleRange.x, helicopterConfig.scaleRange.y);
                float scaleZ = Random.Range(helicopterConfig.scaleRange.x, helicopterConfig.scaleRange.y);
                componentTransform.localScale = Vector3.Scale(componentTransform.localScale, new Vector3(scaleX, scaleY, scaleZ));
            }
        }

        int leftWeaponIndex = Random.Range(0, helicopterConfig.mainWeaponPrefab.Length);
        GameObject leftWeapon = Instantiate(helicopterConfig.mainWeaponPrefab[leftWeaponIndex], helicopter.transform);
        leftWeapon.name = $"Left Weapon {leftWeaponIndex}";
        leftWeapon.transform.position = FindChildByName(helicopter, _leftWeaponRootName).transform.position;
        leftWeapon.transform.localScale *= Random.Range(helicopterConfig.scaleRange.x, helicopterConfig.scaleRange.y);
        
        int rightWeaponIndex = Random.Range(0, helicopterConfig.mainWeaponPrefab.Length);
        GameObject rightWeapon = Instantiate(helicopterConfig.mainWeaponPrefab[rightWeaponIndex], helicopter.transform);
        rightWeapon.name = $"Right Weapon {rightWeaponIndex}";
        rightWeapon.transform.position = FindChildByName(helicopter, _rightWeaponRootName).transform.position;
        rightWeapon.transform.localScale = Vector3.Scale(rightWeapon.transform.localScale, new Vector3(1,1,-1));
        rightWeapon.transform.localScale *= Random.Range(helicopterConfig.scaleRange.x, helicopterConfig.scaleRange.y);
        
        int frontWeaponIndex = Random.Range(0, helicopterConfig.frontWeaponPrefab.Length);
        GameObject frontWeapon = Instantiate(helicopterConfig.frontWeaponPrefab[frontWeaponIndex], helicopter.transform);
        frontWeapon.name = $"Front Weapon {frontWeaponIndex}";
        frontWeapon.transform.position = FindChildByName(helicopter, _frontWeaponRootName).transform.position;
        frontWeapon.transform.localScale *= Random.Range(helicopterConfig.scaleRange.x, helicopterConfig.scaleRange.y);
        
        foreach (ImplicitSurface implicitSurface in helicopter.GetComponentsInChildren<ImplicitSurface>())
        {
            if (!implicitSurface.gameObject.name.Contains(_disableRandomTag))
            {
                implicitSurface.bound = Vector3.Scale(implicitSurface.bound, helicopterConfig.scaleRange.y * Vector3.one);
            }
            
            Material material = new Material(implicitSurface.material);
            material.color = Color.HSVToRGB(Random.value, 0.5f, 1f);
            implicitSurface.material = material;
        }
        
        ImplicitSurfaceManager implicitSurfaceManager = helicopter.GetComponent<ImplicitSurfaceManager>();
        implicitSurfaceManager.Polygonize();
        
        Random.state = state;
        return helicopter;
    }
    
    private GameObject FindChildByName(GameObject parent, string childName)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.gameObject.name == childName)
                return child.gameObject;
            
            GameObject result = FindChildByName(child.gameObject, childName);
            if (result != null)
                return result;
        }
        return null;
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
