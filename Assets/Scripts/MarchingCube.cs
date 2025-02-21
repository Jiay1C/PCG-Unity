using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class MarchingCube : MonoBehaviour
{
    public static MarchingCube Instance { get; private set; }

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
                    Vector3 pos = new Vector3(x,y,z) / resolution + minBound;
                    grid[x, y, z] = sdf(pos);
                    // if (grid[x, y, z] < 0)
                    // {
                    //     Gizmos.DrawSphere(pos, 0.1f / resolution);
                    // }
                }
            }
        }
        
        int[,,,] edgePointIndex = new int[gridSizeX, gridSizeY, gridSizeZ, 3];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    for (int w = 0; w < 3; w++)
                    {
                        edgePointIndex[x, y, z, w] = -1;
                    }
                }
            }
        }
        
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        for (int x = 0; x < gridSizeX - 1; x++)
        {
            for (int y = 0; y < gridSizeY - 1; y++)
            {
                for (int z = 0; z < gridSizeZ - 1; z++)
                {
                    Vector3[] pos = new Vector3[8];

                    int cubeIndex = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int o = _pointOffset[i];
                        int ox = x + o.x;
                        int oy = y + o.y;
                        int oz = z + o.z;
                        
                        pos[i] = new Vector3(ox,oy,oz) / resolution + minBound;
                        
                        if (grid[ox, oy, oz] <= 0)
                        {
                            cubeIndex |= 1 << i;
                        }
                    }

                    int[] pointOnEdges = new int[12];
                    int intersectEdges = _edgeTable[cubeIndex];
                    int edgeIndex = 0;
                    while (intersectEdges > 0)
                    {
                        if ((intersectEdges & 1) >= 1)
                        {
                            (var edgeGlobalIndex, var edgeDir) = _edgeIndex[edgeIndex];
                            edgeGlobalIndex += new Vector3Int(x, y, z);
                            int pointIndex = edgePointIndex[edgeGlobalIndex.x, edgeGlobalIndex.y, edgeGlobalIndex.z, edgeDir];
                            if (pointIndex >= 0)
                            {
                                pointOnEdges[edgeIndex] = pointIndex;
                            }
                            else
                            {
                                int i0 = _edgePoint[edgeIndex].x;
                                int i1 = _edgePoint[edgeIndex].y;
                            
                                Vector3 p0 = pos[i0];
                                Vector3 p1 = pos[i1];

                                Vector3Int ov0 = new Vector3Int(x, y, z) + _pointOffset[i0];
                                Vector3Int ov1 = new Vector3Int(x, y, z) + _pointOffset[i1];
                            
                                float v0 = Mathf.Abs(grid[ov0.x, ov0.y, ov0.z]);
                                float v1 = Mathf.Abs(grid[ov1.x, ov1.y, ov1.z]);
                            
                                vertices.Add(Vector3.Lerp(p0, p1, v0 / (v0 + v1)));
                                
                                uvs.Add(Vector3.Lerp(p0, p1, v0 / (v0 + v1)));
                                
                                pointIndex = vertices.Count - 1;
                                pointOnEdges[edgeIndex] = pointIndex;
                                edgePointIndex[edgeGlobalIndex.x, edgeGlobalIndex.y, edgeGlobalIndex.z, edgeDir] = pointIndex;
                            }
                        }

                        edgeIndex++;
                        intersectEdges >>= 1;
                    }

                    for (int i = 0; i < 16; i++)
                    {
                        int pointIndex = _triangleTable[cubeIndex, i];
                        if (pointIndex < 0)
                        {
                            break;
                        }
                        triangles.Add(pointOnEdges[pointIndex]);
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
        mesh.RecalculateUVDistributionMetrics();

        return mesh;
    }

    // private void OnDrawGizmos()
    // {
    //     float size = 10f;
    //     float res = 1f;
    //     Generate((x) => { return x.magnitude - 5; }, -Vector3.one*size, Vector3.one*size, res);
    // }

    // private void Start()
    // {
    //     float size = 10f;
    //     float res = 2f;
    //     Mesh mesh = Generate((x) => { return x.magnitude - 5; }, -Vector3.one*size, Vector3.one*size, res);
    //     
    //     GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //     go.GetComponent<MeshFilter>().mesh = mesh;
    // }

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