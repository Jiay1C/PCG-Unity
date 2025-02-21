using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance { get; private set; }
    
    public Material terrainMaterial;
    public float terrainUVRatio = 0.08f;

    public float refreshDistance = 60f;
    public float refreshSeconds = 0.5f;
    
    public float[] lodDistance = {100f, 120f, 150f, 200f, 300f, 500f};
    public float[] lodResolution = {1, 0.8f, 0.6f, 0.4f, 0.2f, 0.1f};

    public int randomSeed = 123;
    
    public float frequency = 0.04f;
    public float[] heightAmplitude = {8f, 4f, 2f};
    
    private GameObject _terrain;
    private Vector3 _cameraPos;
    private Vector2[] _perlinOffset;
    
    public void DestroyTerrain()
    {
        if (_terrain != null)
        {
            Destroy(_terrain);
        }
    }

    public void CreateTerrain()
    {
        DestroyTerrain();
        _terrain = new GameObject("Terrain");
        
        if (lodDistance.Length != lodResolution.Length)
        {
            throw new System.ArgumentException("LOD config invalid.");
        }
        
        long[] timer = new long[lodDistance.Length];
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        
        CreateTerrainLOD0();
        timer[0] = stopwatch.ElapsedMilliseconds;

        for (int i = 1; i < lodDistance.Length; i++)
        {
            CreateTerrainLODX(i);
            timer[i] = stopwatch.ElapsedMilliseconds;
        }
        
        stopwatch.Stop();
        
        StringBuilder log = new StringBuilder();
        log.Append($"[Terrain] Create Terrain ({stopwatch.ElapsedMilliseconds} ms)\n");
        log.Append($"Details: LOD{0} = {timer[0]} | ");
        for (int i = 1; i < timer.Length; i++)
        {
            log.Append($"LOD{i} = {timer[i] - timer[i - 1]} | ");
        }
        
        Debug.Log(log.ToString());
    }

    public void UpdateTerrain()
    {
        Vector3 cameraPos = CameraController.Instance.CameraPosition;
        if ((cameraPos - _cameraPos).magnitude <= refreshDistance)
        {
            return;
        }
        
        _cameraPos = cameraPos;
        CreateTerrain();
    }

    private void CreateTerrainLOD0()
    {
        int count = (int)(2 * (lodDistance[0] * lodResolution[0]));
        float[,] heightMap = new float[count + 1, count + 1];
        Vector3 offset = new Vector3(-lodDistance[0] + _cameraPos.x, 0f, -lodDistance[0] + _cameraPos.z);
        
        for (int z = 0; z <= count; z++)
        {
            for (int x = 0; x <= count; x++)
            {
                float perlinX = z / lodResolution[0] + offset.z;
                float perlinY = x / lodResolution[0] + offset.x;
                heightMap[z, x] = PerlinNoise(perlinX, perlinY);
            }
        }
        
        Mesh mesh = Util.ConvertHeightMapToMesh(heightMap, offset, terrainUVRatio, 1.0f / lodResolution[0], 1.0f);

        CreateTerrainObject("TerrainLOD0", mesh);
    }

    private void CreateTerrainLODX(int lod)
    {
        if (lod >= lodDistance.Length || lod <= 0)
        {
            throw new System.IndexOutOfRangeException("LOD is out of range.");
        }

        int pLod = lod - 1;
        
        float distanceSmall = lodDistance[lod] - lodDistance[pLod];
        float distanceLarge = lodDistance[lod] + lodDistance[pLod];
        Vector2[] distance =
        {
            new Vector2(distanceLarge, distanceSmall),
            new Vector2(distanceSmall, distanceLarge),
            new Vector2(distanceLarge, distanceSmall),
            new Vector2(distanceSmall, distanceLarge),
        };
        Vector3[] offsets =
        {
            new Vector3(-lodDistance[pLod] + _cameraPos.x, 0f,  lodDistance[pLod] + _cameraPos.z),
            new Vector3( lodDistance[pLod] + _cameraPos.x, 0f, -lodDistance[lod] + _cameraPos.z),
            new Vector3(-lodDistance[lod] + _cameraPos.x, 0f, -lodDistance[lod] + _cameraPos.z),
            new Vector3(-lodDistance[lod] + _cameraPos.x, 0f, -lodDistance[pLod] + _cameraPos.z),
        };
        
        CombineInstance[] meshes = new CombineInstance[4];
        
        for (int i = 0; i < 4; i++)
        {
            int countX = (int)(distance[i].x * lodResolution[lod]);
            int countZ = (int)(distance[i].y * lodResolution[lod]);
            
            float[,] heightMap = new float[countZ + 1, countX + 1];
            for (int z = 0; z <= countZ; z++)
            {
                for (int x = 0; x <= countX; x++)
                {
                    float perlinX = z / lodResolution[lod] + offsets[i].z;
                    float perlinY = x / lodResolution[lod] + offsets[i].x;
                    heightMap[z, x] = PerlinNoise(perlinX, perlinY);
                }
            }
            
            meshes[i].mesh = Util.ConvertHeightMapToMesh(heightMap, offsets[i], terrainUVRatio, 1.0f / lodResolution[lod], 1.0f);
            meshes[i].transform = Matrix4x4.identity;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.CombineMeshes(meshes, true);
        CreateTerrainObject($"TerrainLOD{lod}", mesh);
    }

    private void CreateTerrainObject(string name, Mesh mesh)
    {
        GameObject obj = new GameObject(name,typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        obj.transform.parent = _terrain.transform;
        
        MeshFilter filter = obj.GetComponent<MeshFilter>();
        filter.mesh = mesh;
        
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        renderer.material = (terrainMaterial != null) ? terrainMaterial : new Material(Shader.Find("Standard"));
        
        MeshCollider collider = obj.GetComponent<MeshCollider>();
        collider.sharedMesh = filter.mesh;
    }

    private float PerlinNoise(float x, float y)
    {
        float result = 0;
        float currentFrequency = frequency;

        int amplitudeCount = heightAmplitude.Length;
        for (int i = 0; i < amplitudeCount; i++)
        {
            float perlinX = x * currentFrequency + _perlinOffset[i].x;
            float perlinY = y * currentFrequency + _perlinOffset[i].y;
            result += heightAmplitude[i] * Mathf.PerlinNoise(perlinX, perlinY);
            currentFrequency *= 2;
        }
        
        return result;
    }
    
    private void InitPerlinNoise()
    {
        int maxOffset = 1000;
        Random.InitState(randomSeed);
        
        int amplitudeCount = heightAmplitude.Length;
        _perlinOffset = new Vector2[amplitudeCount];
        for (int i = 0; i < amplitudeCount; i++)
        {
            _perlinOffset[i].Set((Random.value - 0.5f) * maxOffset, (Random.value - 0.5f) * maxOffset);
        }
    }

    private IEnumerator TerrainUpdater()
    {
        while (Application.isPlaying)
        {
            UpdateTerrain();
            yield return new WaitForSeconds(refreshSeconds);
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
        InitPerlinNoise();
        CreateTerrain();
        StartCoroutine(TerrainUpdater());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
