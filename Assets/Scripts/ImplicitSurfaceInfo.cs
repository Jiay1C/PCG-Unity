using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImplicitSurfaceInfo : MonoBehaviour
{
    public enum SurfaceType
    {
        None,
        Sphere,
        Box,
        Capsule,
    }
    
    public SurfaceType surfaceType = SurfaceType.None;
    public ImplicitSurface.SDFMode mode = ImplicitSurface.SDFMode.None;
    
    public ImplicitSurface.SDFType GetSDF()
    {
        switch (surfaceType)
        {
            case SurfaceType.Sphere:
                return SphereSDF;
            case SurfaceType.Box:
                return BoxSDF;
            case SurfaceType.Capsule:
                return CapsuleSDF;
        }

        return VoidSDF;
    }

    public ImplicitSurface.SDFMode GetSDFMode()
    {
        return mode;
    }
    
    private float VoidSDF(Vector3 position)
    {
        return float.PositiveInfinity;
    }

    private float SphereSDF(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        return position.magnitude - 1;
    }

    private float BoxSDF(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        float radius = 0.1f;
        
        Vector3 q = Vector3.Max(new Vector3(Mathf.Abs(position.x), Mathf.Abs(position.y), Mathf.Abs(position.z)) - (1.0f - radius) * Vector3.one, Vector3.zero);
        return q.magnitude + Mathf.Min(Mathf.Max(q.x, Mathf.Max(q.y, q.z)), 0.0f) - radius;
    }
    
    private float CapsuleSDF(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        
        Vector3 a = new Vector3(0, -0.5f, 0);
        Vector3 b = new Vector3(0, 0.5f, 0);
        float radius = 1f;
        
        Vector3 pa = position - a;
        Vector3 ba = b - a;
        float h = Mathf.Clamp(Vector3.Dot(pa, ba) / Vector3.Dot(ba, ba), 0.0f, 1.0f);
        return (pa - ba * h).magnitude - radius;
    }
    
}
