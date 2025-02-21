using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class DualContouring : MonoBehaviour
{
    public static DualContouring Instance { get; private set; }

    public float NormalDelta = 0.001f;
    public int SchmitzInteration = 4;
    public float SchmitzStepSize = 0.1f;

    public Mesh Generate(Func<Vector3, float> sdf, Vector3 minBound, Vector3 maxBound, float resolution)
    {
        int gridSizeX = (int)((maxBound.x - minBound.x) * resolution);
        int gridSizeY = (int)((maxBound.y - minBound.y) * resolution);
        int gridSizeZ = (int)((maxBound.z - minBound.z) * resolution);
        float[,,] grid = new float[gridSizeX, gridSizeY, gridSizeZ];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 pos = new Vector3(x, y, z) / resolution + minBound;
                    grid[x, y, z] = sdf(pos);
                }
            }
        }
        
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        
        int[,,] gridVertexIndex = new int[gridSizeX - 1, gridSizeY - 1, gridSizeZ - 1];
        
        for (int x = 0; x < gridSizeX - 1; x++)
        {
            for (int y = 0; y < gridSizeY - 1; y++)
            {
                for (int z = 0; z < gridSizeZ - 1; z++)
                {
                    List<(Vector3, Vector3)> intersections = new List<(Vector3, Vector3)>();
                    foreach (var edge in _cellEdge)
                    {
                        Vector3Int p0 = _cellPoint[edge.x] + new Vector3Int(x, y, z);
                        Vector3Int p1 = _cellPoint[edge.y] + new Vector3Int(x, y, z);
                        
                        float p0Value = grid[p0.x, p0.y, p0.z];
                        float p1Value = grid[p1.x, p1.y, p1.z];
                        
                        if (p0Value <= 0 && p1Value >= 0 || p0Value >= 0 && p1Value <= 0)
                        {
                            float interpolateFactor = Mathf.Abs(p0Value) / (Mathf.Abs(p0Value) + Mathf.Abs(p1Value));
                            Vector3 p = Vector3.Lerp(_cellPoint[edge.x], _cellPoint[edge.y], interpolateFactor);
                            intersections.Add((p, CalculateNormal(sdf, (p + new Vector3Int(x, y, z)) / resolution + minBound)));
                        }
                    }

                    if (intersections.Count > 0)
                    {
                        // TODO: Calc Vertex Pos
                        // Currently place vertex in the middle of cell
                        
                        Vector3 center = Vector3.zero;
                        foreach ((var position, var normal) in intersections)
                        {
                            center += position;
                        }
                        center /= intersections.Count;
                        
                        for (int step = 0; step < SchmitzInteration; step++)
                        {
                            Vector3[] force = new Vector3[8];
                            
                            for (int i = 0; i < 8; i++)
                            {
                                foreach ((var position, var normal) in intersections)
                                {
                                    float distance = Vector3.Dot(normal, _cellPoint[i] - position);
                                    Vector3 corner2Plane = -distance * normal;
                                    force[i] += corner2Plane;
                                }
                            }
                        
                            Vector3 force00 = Vector3.Lerp(force[0], force[1], center.x);
                            Vector3 force01 = Vector3.Lerp(force[3], force[2], center.x);
                            Vector3 force02 = Vector3.Lerp(force[4], force[5], center.x);
                            Vector3 force03 = Vector3.Lerp(force[7], force[6], center.x);
                        
                            Vector3 force10 = Vector3.Lerp(force00, force02, center.y);
                            Vector3 force11 = Vector3.Lerp(force01, force03, center.y);
                        
                            Vector3 force20 = Vector3.Lerp(force10, force11, center.z);

                            center += force20 * SchmitzStepSize;
                        }
                        
                        gridVertexIndex[x, y, z] = vertices.Count;
                        vertices.Add((center + new Vector3(x, y, z)) / resolution + minBound);
                        uvs.Add((center + new Vector3(x, y, z)) / resolution + minBound);
                    }
                    else
                    {
                        gridVertexIndex[x, y, z] = -1;
                    }
                }
            }
        }
        
        List<int> triangles = new List<int>();
        
        for (int x = 1; x < gridSizeX - 1; x++)
        {
            for (int y = 1; y < gridSizeY - 1; y++)
            {
                for (int z = 1; z < gridSizeZ - 1; z++)
                {
                    foreach (var (offset, cellOffset) in _pointOffset)
                    {
                        Vector3Int p0 = new Vector3Int(x, y, z);
                        Vector3Int p1 = p0 + offset;
                        
                        float p0Value = grid[p0.x, p0.y, p0.z];
                        float p1Value = grid[p1.x, p1.y, p1.z];
                        
                        
                        if (p0Value <= 0 && p1Value >= 0 || p0Value >= 0 && p1Value <= 0)
                        {
                            int[] vertexIndex = new int[4];

                            for (int i = 0; i < 4; i++)
                            {
                                Vector3Int p = p0 + cellOffset[i];
                                vertexIndex[i] = gridVertexIndex[p.x, p.y, p.z];
                                if (vertexIndex[i] < 0)
                                {
                                    Debug.LogError("Invalid Vertex Index");
                                }
                            }
                            
                            if (p0Value >= 0 && p1Value <= 0)
                            {
                                triangles.Add(vertexIndex[0]);
                                triangles.Add(vertexIndex[2]);
                                triangles.Add(vertexIndex[1]);
                            
                                triangles.Add(vertexIndex[1]);
                                triangles.Add(vertexIndex[2]);
                                triangles.Add(vertexIndex[3]);
                            }
                            else if (p0Value <= 0 && p1Value >= 0)
                            {
                                triangles.Add(vertexIndex[0]);
                                triangles.Add(vertexIndex[1]);
                                triangles.Add(vertexIndex[2]);
                            
                                triangles.Add(vertexIndex[1]);
                                triangles.Add(vertexIndex[3]);
                                triangles.Add(vertexIndex[2]);
                            }
                        }
                    }
                }
            }
        }
        
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        
        // mesh.Optimize();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        // mesh.RecalculateUVDistributionMetrics();

        return mesh;
    }

    private Vector3 CalculateNormal(Func<Vector3, float> sdf, Vector3 position)
    {
        return new Vector3(
            (sdf(position + new Vector3(NormalDelta, 0, 0)) - sdf(position - new Vector3(NormalDelta, 0, 0))) / 2 / NormalDelta,
            (sdf(position + new Vector3(0, NormalDelta, 0)) - sdf(position - new Vector3(0, NormalDelta, 0))) / 2 / NormalDelta,
            (sdf(position + new Vector3(0, 0, NormalDelta)) - sdf(position - new Vector3(0, 0, NormalDelta))) / 2 / NormalDelta
        ).normalized;
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
