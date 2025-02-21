using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class MeshGenerator : MonoBehaviour
{
    public static MeshGenerator Instance { get; private set; }
    
    public BushConfig[] bushConfig;
    public LeafConfig[] leafConfig;

    public (Mesh, Mesh) Bush(int seed, int branchType = -1, int leafType = -1)
    {
        Random.State state = Random.state;
        Random.InitState(seed);

        if (branchType < 0 || branchType >= bushConfig.Length)
        {
            branchType = Random.Range(0, bushConfig.Length);
        }
        
        if (leafType < 0 || leafType >= leafConfig.Length)
        {
            leafType = Random.Range(0, leafConfig.Length);
        }
        
        BushConfig config = bushConfig[branchType];
        
        if (config == null || !config.Valid())
        {
            throw new ArgumentException($"Invalid Bush Config {branchType}");
        }
        
        List<CombineInstance> branchMeshes = new List<CombineInstance>();
        List<CombineInstance> leafMeshes = new List<CombineInstance>();
        Queue<(int, Vector3[], float, Quaternion)> branchInfo = new Queue<(int, Vector3[], float, Quaternion)>();
        branchInfo.Enqueue((-1, new []{Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero}, config.BranchRadius, Quaternion.identity));
        
        for (int i = 0; i < config.MaxDegree; i++)
        {
            while (branchInfo.Count > 0)
            {
                (var degree, var points, var radius, var rotation) = branchInfo.Peek();

                if (degree != i - 1)
                {
                    break;
                }

                // Generate Sub Branch
                for (float t = config.BranchSpawnRange[i].x; t < config.BranchSpawnRange[i].y; t += Mathf.Lerp(config.BranchDistance[i].x, config.BranchDistance[i].y, Random.value))
                {
                    float newRadius = radius * (1 - t) * config.BranchRadiusRatio;
                    if (newRadius < config.CircleSegment * config.MinSegment)
                    {
                        break;
                    }
                    
                    if (Random.value > config.Probability[i])
                    {
                        continue;
                    }
                    
                    (var newMesh, var newPoints) = BushBranch(branchType, i, newRadius);
                    
                    Vector3 newTranslation = Bezier.Point(points, t);
                    Quaternion newRotation = Quaternion.Euler(Mathf.Lerp(config.BranchPitchRange[i].x, config.BranchPitchRange[i].y, Random.value), Random.value * 360.0f, 0.0f) * rotation;
                    Matrix4x4 newTrans = Matrix4x4.TRS(newTranslation, newRotation, Vector3.one);
                    
                    for (int index = 0; index < 4; index++)
                    {
                        newPoints[index] = newTrans.MultiplyPoint(newPoints[index]);
                    }
                    
                    CombineInstance instance = new CombineInstance();
                    instance.mesh = newMesh;
                    instance.transform = newTrans;
                    
                    branchMeshes.Add(instance);
                    branchInfo.Enqueue((i, newPoints, newRadius, newRotation));
                    
                    // Generate Sub Leaf
                    for (float newT = config.LeafSpawnRange[i].x; newT < config.LeafSpawnRange[i].y; newT += Mathf.Lerp(config.LeafDistance[i].x, config.LeafDistance[i].y, Random.value))
                    {
                        Vector3 leafTranslation = Bezier.Point(newPoints, newT);
                        Quaternion leafRotation = Quaternion.Euler(Mathf.Lerp(config.LeafPitchRange[i].x, config.LeafPitchRange[i].y, Random.value), Random.value * 360.0f, Random.value * 360.0f) * rotation;
                        Matrix4x4 leafTrans = Matrix4x4.TRS(leafTranslation, leafRotation, Vector3.one);
                    
                        Mesh leafMesh = BushLeaf(leafType);
                    
                        CombineInstance leafInstance = new CombineInstance();
                        leafInstance.mesh = leafMesh;
                        leafInstance.transform = leafTrans;
                    
                        leafMeshes.Add(leafInstance);
                    }
                }
                
                branchInfo.Dequeue();
            }
        }
        
        Mesh branchMeshResult = new Mesh();
        branchMeshResult.indexFormat = IndexFormat.UInt32;
        branchMeshResult.CombineMeshes(branchMeshes.ToArray(), true);
        
        Mesh leafMeshResult = new Mesh();
        leafMeshResult.indexFormat = IndexFormat.UInt32;
        leafMeshResult.CombineMeshes(leafMeshes.ToArray(), true);
        
        Random.state = state;
        
        return (branchMeshResult, leafMeshResult);
    }

    private (Mesh, Vector3[]) BushBranch(int type, int index, float radius)
    {
        BushConfig config = bushConfig[type];
        
        float length = Mathf.Lerp(config.BranchLength[index].x, config.BranchLength[index].y, Random.value);
        
        Vector2[] randomPos = new[]
        {
            Random.insideUnitCircle * config.MaxCurve[index],
            Random.insideUnitCircle * config.MaxCurve[index],
            Random.insideUnitCircle * config.MaxCurve[index],
        };
        
        Vector3[] points = new[]
        {
            new Vector3(0, 0, 0),
            new Vector3(randomPos[0].x, 0.5f * length, randomPos[0].y),
            new Vector3(randomPos[1].x, 0.8f * length, randomPos[1].y),
            new Vector3(randomPos[2].x, 1.0f * length, randomPos[2].y),
        };
        
        int lineCount = (int)(length / config.LineSegment);
        int circleCount = (int)(radius / config.CircleSegment);
        
        Mesh mesh = Util.ConvertBezierCurveToMesh(points, lineCount, circleCount, radius, 0, config.UVRatio);
        return (mesh, points);
    }

    private Mesh BushLeaf(int type)
    {
        LeafConfig config = leafConfig[type];
        
        float size = Mathf.Lerp(config.size.x, config.size.y, Random.value);
        float width = Mathf.Lerp(config.width.x, config.width.y, Random.value);
        float center = Mathf.Lerp(config.center.x, config.center.y, Random.value);
        
        Vector3[] points0 = new[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, center * size, 0),
            new Vector3(width * size, center * size, 0),
            new Vector3(0, 1.0f * size, 0),
        };
        
        Vector3[] points1 = new[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, center * size, 0),
            new Vector3(-width * size, center * size, 0),
            new Vector3(0, 1.0f * size, 0),
        };

        CombineInstance[] instance = new CombineInstance[2];
        instance[0].mesh = Util.ConvertBezierShapeToMesh(points0, config.verticesCount, 1.0f, false);
        instance[0].transform = Matrix4x4.identity;
        instance[1].mesh = Util.ConvertBezierShapeToMesh(points1, config.verticesCount, 1.0f, false);
        instance[1].transform = Matrix4x4.identity;

        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.CombineMeshes(instance, true);
        return mesh;
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
