using System;
using UnityEngine;
using UnityEngine.Rendering;

public static class Util
{
    public static Mesh ConvertBezierShapeToMesh(Vector3[] points, int verticesCount, float sizeRatio, bool enableBackFace)
    {
        if (points == null || points.Length != 4)
        {
            throw new ArgumentException("Invalid Points");
        }
        
        float lineDelta = 1.0f / (float)(verticesCount - 1);
        
        Vector3[] vertices = new Vector3[verticesCount + 1];
        // Vector2[] uvs = new Vector2[verticesCount + 1];

        vertices[0] = (points[0] + points[3]) / 2.0f * sizeRatio;
        // uvs[0].Set(0, 0);
        
        for (int line = 0; line < verticesCount; line++)
        {
            float t = line * lineDelta;
            vertices[line + 1] = Bezier.Point(points, t) * sizeRatio;
            // uvs[line + 1].Set(0, 0); // UV Unsupported
        }
        
        int trianglesCount = verticesCount - 1;
        int[] indices = new int[trianglesCount * (enableBackFace ? 6 : 3)];
        for (int i = 0; i < trianglesCount; i++)
        {
            int triangleStartIndex = i * 3;

            indices[triangleStartIndex + 0] = 0;
            indices[triangleStartIndex + 1] = i + 1;
            indices[triangleStartIndex + 2] = i + 2;

            if (enableBackFace)
            {
                triangleStartIndex += trianglesCount * 3;
                
                indices[triangleStartIndex + 0] = 0;
                indices[triangleStartIndex + 1] = i + 2;
                indices[triangleStartIndex + 2] = i + 1;
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices;
        // mesh.uv = uvs;
        mesh.triangles = indices;
        
        // mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        // mesh.RecalculateUVDistributionMetrics();

        return mesh;
    }
    
    public static Mesh ConvertBezierCurveToMesh(Vector3[] points, int lineCount, int circleCount, float startRadius, float endRadius, Vector2 uvRatio)
    {
        if (points == null || points.Length != 4)
        {
            throw new ArgumentException("Invalid Points");
        }
        
        const float PI2 = Mathf.PI * 2f;
        float lineDelta = 1.0f / (float)(lineCount - 1);
        float circleDelta = PI2 / (float)(circleCount - 1);
        
        int verticesCount = lineCount * circleCount;
        int trianglesCount = (lineCount - 1) * (circleCount - 1) * 2;
        
        Vector3[] vertices = new Vector3[verticesCount];
        Vector2[] uvs = new Vector2[verticesCount];
        
        for (int line = 0; line < lineCount; line++)
        {
            float t = line * lineDelta;
            Vector3 center = Bezier.Point(points, t);
            (Vector3 tangent, Vector3 normal, Vector3 biNormal) = Bezier.TNB(points, t);
            
            for (int circle = 0; circle < circleCount; circle++)
            {
                float theta = circle * circleDelta;
                float radius = Mathf.Lerp(startRadius, endRadius, t);
                
                Vector3 pos = center + (normal * Mathf.Cos(theta) + biNormal * Mathf.Sin(theta)) * radius;
                
                vertices[line * circleCount + circle] = pos;
                
                float uPos = uvRatio.x * line / (lineCount - 1);
                float vPos = uvRatio.y * circle / (circleCount - 1);
                uvs[line * circleCount + circle].Set(uPos, vPos);
            }
        }
        
        int[] indices = new int[trianglesCount * 3];

        for (int line = 0; line < lineCount - 1; line++)
        {
            for (int circle = 0; circle < circleCount - 1; circle++)
            {
                int gridId = line * (circleCount - 1) + circle;
                gridId *= 6;
                
                int index0 = line * circleCount + circle;
                int index1 = line * circleCount + circle + 1;
                int index2 = (line + 1) * circleCount + circle;
                int index3 = (line + 1) * circleCount + circle + 1;
                
                indices[gridId + 0] = index0;
                indices[gridId + 1] = index1;
                indices[gridId + 2] = index2;
                indices[gridId + 3] = index1;
                indices[gridId + 4] = index3;
                indices[gridId + 5] = index2;
                
            }
        }
        
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = indices;
        
        // mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateUVDistributionMetrics();

        return mesh;
    }
    
    public static Mesh ConvertHeightMapToMesh(float[,] heightMap, Vector3 offset, float uvRatio, float horizontalRatio, float verticalRatio)
    {
        int zCount = heightMap.GetLength(0);
        int xCount = heightMap.GetLength(1);

        int verticesCount = xCount * zCount;
        int trianglesCount = (xCount - 1) * (zCount - 1) * 2;

        Vector3[] vertices = new Vector3[verticesCount];
        Vector2[] uvs = new Vector2[verticesCount];

        for (int z = 0; z < zCount; z++)
        {
            for (int x = 0; x < xCount; x++)
            {
                float xPos = offset.x + x * horizontalRatio;
                float yPos = offset.y + heightMap[z, x] * verticalRatio;
                float zPos = offset.z + z * horizontalRatio;
                vertices[z * xCount + x].Set(xPos, yPos, zPos);
                
                float uPos = xPos * uvRatio;
                float vPos = zPos * uvRatio;
                uvs[z * xCount + x].Set(uPos, vPos);
            }
        }
        
        int[] indices = new int[trianglesCount * 3];

        for (int z = 0; z < zCount - 1; z++)
        {
            for (int x = 0; x < xCount - 1; x++)
            {
                int gridId = z * (xCount - 1) + x;
                gridId *= 6;
                
                int index0 = z * xCount + x;
                int index1 = z * xCount + x + 1;
                int index2 = (z + 1) * xCount + x;
                int index3 = (z + 1) * xCount + x + 1;
                
                indices[gridId + 0] = index0;
                indices[gridId + 1] = index2;
                indices[gridId + 2] = index1;
                indices[gridId + 3] = index1;
                indices[gridId + 4] = index2;
                indices[gridId + 5] = index3;
                
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = indices;
        
        // mesh.Optimize();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        mesh.RecalculateUVDistributionMetrics();

        return mesh;
    }
}
