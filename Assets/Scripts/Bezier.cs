using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class Bezier
{
    public static Vector3 Point(Vector3[] points, float t)
    {
        float c0 = (1 - t) * (1 - t) * (1 - t);
        float c1 = 3 * (1 - t) * (1 - t) * t;
        float c2 = 3 * (1 - t) * t * t;
        float c3 = t * t * t;
        
        return c0 * points[0] + c1 * points[1] + c2 * points[2] + c3 * points[3];
    }

    public static Vector3 Tangent(Vector3[] points, float t)
    {
        float c0 = -3 * (1 - t) * (1 - t);
        float c1 = 3 * (1 - t) * (1 - t) - 6 * t * (1 - t);
        float c2 = 6 * t * (1 - t) - 3 * t * t;
        float c3 = 3 * t * t;
        
        return c0 * points[0] + c1 * points[1] + c2 * points[2] + c3 * points[3];
    }

    public static Vector3 Normal(Vector3[] points, float t)
    {
        Vector3 tangent = Tangent(points, t);
        return Vector3.Cross(tangent, Vector3.forward).normalized;
    }

    public static Vector3 BiNormal(Vector3[] points, float t)
    {
        Vector3 tangent = Tangent(points, t);
        Vector3 normal = Normal(points, t);
        
        return Vector3.Cross(tangent, normal).normalized;
    }

    public static (Vector3, Vector3, Vector3) TNB(Vector3[] points, float t)
    {
        Vector3 tangent = Tangent(points, t).normalized;
        Vector3 normal = Vector3.Cross(tangent, Vector3.forward).normalized;
        Vector3 biNormal = Vector3.Cross(tangent, normal).normalized;

        return (tangent, normal, biNormal);
    }
}
