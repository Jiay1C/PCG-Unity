using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class FoliageSystem : MonoBehaviour
{
    public static FoliageSystem Instance { get; private set; }
    public int randomSeed = 0;
    public Material plantMaterial;
    public Material rockMaterial;
    public Material bushMaterial;
    public Material leafMaterial;
    public Color[] bushColors = new Color[] { Color.white };
    public Color[] leafColors = new Color[] { Color.white };
    public float blockSize = 100f;
    public int availableBlockDistance = 2;
    public int plantsPerBlock = 40;
    public int rocksPerBlock = 20;
    public int bushesPerBlock = 5;
    public int minObjectsPerRock = 3;
    public int maxObjectsPerRock = 10;
    public float foliageSize = 0.8f;
    public float maxHeight = 12f;
    public float updateFrequency = 10f;
    
    private enum FoliageType {Plant, Rock, Bush}

    private GameObject _foliageRoot;
    private Dictionary<Vector2Int, GameObject> _foliageObjects;
    private float _updateIntervalSeconds;

    public void DisableFoliage()
    {
        StopAllCoroutines();
        if (_foliageObjects != null)
        {
            _foliageObjects.Clear();
            _foliageObjects = null;
        }
        Destroy(_foliageRoot);
        _foliageRoot = null;
    }

    public void EnableFoliage()
    {
        DisableFoliage();
        Random.InitState(randomSeed);
        _foliageRoot = new GameObject("Foliage");
        _foliageRoot.transform.position = Vector3.zero;
        _foliageObjects = new Dictionary<Vector2Int, GameObject>();
        StartCoroutine(FoliageUpdater());
    }
    
    private IEnumerator FoliageUpdater()
    {
        while (Application.isPlaying)
        {
            Vector3 cameraPos = CameraController.Instance.CameraPosition;
            
            Vector2Int currentBlock = new Vector2Int((int)(cameraPos.x / blockSize), (int)(cameraPos.z / blockSize));
            
            foreach (var (block, plantRoot) in _foliageObjects.ToList())
            {
                if ((block - currentBlock).sqrMagnitude > 3 * availableBlockDistance * availableBlockDistance)
                {
                    Destroy(plantRoot);
                    _foliageObjects.Remove(block);
                    Debug.Log($"[Foliage] Remove Foliage Block [{block.x}, {block.y}] ");
                }
            }

            
            for (int i = 0; i <= availableBlockDistance << 1; i++)
            {
                for (int j = 0; j <= availableBlockDistance << 1; j++)
                {
                    int offsetX = (i + 1) / 2 * (i % 2 == 0 ? -1 : 1);
                    int offsetY = (j + 1) / 2 * (j % 2 == 0 ? -1 : 1);
                    
                    Vector2Int thisBlock = currentBlock + new Vector2Int(offsetX, offsetY);
                    if (!_foliageObjects.ContainsKey(thisBlock))
                    {
                        var stopwatch = new System.Diagnostics.Stopwatch();
                        stopwatch.Start();
                        
                        GameObject rockSubRoot = new GameObject($"Foliage Block [{thisBlock.x}, {thisBlock.y}]");
                        rockSubRoot.transform.parent = _foliageRoot.transform;
                        rockSubRoot.transform.localPosition = new Vector3(thisBlock.x * blockSize, 0f, thisBlock.y * blockSize);
                        long rootCost = stopwatch.ElapsedMilliseconds;
                        
                        for (int k = 0; k < plantsPerBlock; k++)
                        {
                            FoliagePlacer(rockSubRoot, FoliageType.Plant);
                        }
                        long plantCost = stopwatch.ElapsedMilliseconds - rootCost;

                        for (int k = 0; k < rocksPerBlock; k++)
                        {
                            FoliagePlacer(rockSubRoot, FoliageType.Rock);
                        }
                        long rockCost = stopwatch.ElapsedMilliseconds - plantCost;
                        
                        for (int k = 0; k < bushesPerBlock; k++)
                        {
                            FoliagePlacer(rockSubRoot, FoliageType.Bush);
                        }
                        long bushCost = stopwatch.ElapsedMilliseconds - rockCost;
                        
                        _foliageObjects.Add(thisBlock, rockSubRoot);
                        
                        stopwatch.Stop();
                        Debug.Log($"Generate Foliage Block [{thisBlock.x}, {thisBlock.y}] ({stopwatch.ElapsedMilliseconds} ms)\nDetails: Root = {rootCost}ms | Plant = {plantCost}ms | Rock = {rockCost}ms | Bush = {bushCost}ms");

                        yield return null;
                    }
                }
            }
            
            yield return new WaitForSeconds(_updateIntervalSeconds);
        }
    }

    private void FoliagePlacer(GameObject blockRoot, FoliageType type)
    {
        GameObject foliageObject = new GameObject($"Foliage - {type}");
        foliageObject.transform.parent = blockRoot.transform;
        foliageObject.transform.localScale = Vector3.one * foliageSize;
        
        foliageObject.transform.localPosition = new Vector3((Random.value - 0.5f) * blockSize, maxHeight, (Random.value - 0.5f) * blockSize);
        if (Physics.Raycast(foliageObject.transform.position, Vector3.down, out var hit))
        {
            foliageObject.transform.position = hit.point;
            switch (type)
            {
                case FoliageType.Plant:
                    PlantGenerator(foliageObject);
                    break;
                case FoliageType.Rock:
                    RockGenerator(foliageObject);
                    break;
                case FoliageType.Bush:
                    BushGenerator(foliageObject);
                    break;
                default:
                    Destroy(foliageObject);
                    throw new Exception("Unsupported Foliage Type");
            }
        }
        else
        {
            Destroy(foliageObject);
        }
    }

    private void PlantGenerator(GameObject root)
    {
        GameObject plantComponent = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        plantComponent.name = "Plant Component";
        plantComponent.transform.parent = root.transform;
        plantComponent.transform.localPosition = Vector3.zero;
        plantComponent.transform.localScale = new Vector3(1f, 2.4f, 1f);
        plantComponent.GetComponent<Renderer>().material = plantMaterial;
    }
    
    private void RockGenerator(GameObject root)
    {
        int objectsPerRock = Random.Range(minObjectsPerRock, maxObjectsPerRock);
        for (int i = 0; i < objectsPerRock; i++)
        {
            GameObject rockComponent = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rockComponent.name = $"Rock Component {i}";
            rockComponent.transform.parent = root.transform;
            rockComponent.transform.localPosition = Vector3.up + Random.insideUnitSphere;
            rockComponent.transform.localScale = new Vector3(0.4f + Random.value * 0.8f, 0.06f + Random.value * 0.08f, 0.4f + Random.value * 0.8f);
            rockComponent.GetComponent<Renderer>().material = rockMaterial;
            rockComponent.AddComponent<Rigidbody>();
            Destroy(rockComponent.GetComponent<Collider>());
            rockComponent.AddComponent<BoxCollider>();
        }
    }

    private void BushGenerator(GameObject root)
    {
        GameObject bush = new GameObject($"Bush Component");
        bush.transform.parent = root.transform;
        bush.transform.localPosition = Vector3.zero;
        
        (Mesh branchMesh, Mesh leafMesh) = MeshGenerator.Instance.Bush(Random.Range(0, int.MaxValue));
        
        GameObject branchComponent = new GameObject($"Branch Component", typeof(MeshFilter), typeof(MeshRenderer));
        branchComponent.transform.parent = bush.transform;
        branchComponent.transform.localPosition = Vector3.zero;
        branchComponent.GetComponent<MeshFilter>().mesh = branchMesh;
        Material bMaterial = new Material(bushMaterial);
        bMaterial.color = bushColors[Random.Range(0, bushColors.Length)];
        branchComponent.GetComponent<MeshRenderer>().material = bMaterial;
        
        GameObject leafComponent = new GameObject($"Leaf Component", typeof(MeshFilter), typeof(MeshRenderer));
        leafComponent.transform.parent = bush.transform;
        leafComponent.transform.localPosition = Vector3.zero;
        leafComponent.GetComponent<MeshFilter>().mesh = leafMesh;
        Material lMaterial = new Material(leafMaterial);
        lMaterial.color = leafColors[Random.Range(0, leafColors.Length)];
        leafComponent.GetComponent<MeshRenderer>().material = lMaterial;
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

        _updateIntervalSeconds = 1.0f / updateFrequency;
    }

    private void Start()
    {
        EnableFoliage();
    }

    private void OnDisable()
    {
        DisableFoliage();
    }
}
