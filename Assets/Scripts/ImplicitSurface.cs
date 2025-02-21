using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImplicitSurface : MonoBehaviour
{
    public delegate float SDFType(Vector3 position);
    
    public enum SDFMode
    {
        None,
        Add,
        Subtract,
    }

    public enum ConvertMode
    {
        MarchingCube,
        DualContouring,
    }

    public ConvertMode convertMode;
    public Material material;
    public Vector3 bound = new (5, 5, 5);
    public float resolution = 4f;
    public float smoothness = 1f;
    
    private List<(SDFType, SDFMode)> _sdfList;
    private GameObject _implicitSurface;
    private Coroutine _refreshCoroutine;
    
    public void Polygonize()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        Remove();
        
        _sdfList = new List<(SDFType, SDFMode)>();
        FetchSDF(gameObject);

        Mesh mesh;
        
        switch (convertMode)
        {
            case ConvertMode.MarchingCube:
                mesh = MarchingCube.Instance.Generate(SDF, -bound, bound, resolution);
                break;
            case ConvertMode.DualContouring:
                mesh = DualContouring.Instance.Generate(SDF, -bound, bound, resolution);
                break;
            default:
                throw new Exception("Implicit Surface not implemented");
        }
        
        _implicitSurface = new GameObject($"{gameObject.name} Mesh", typeof(MeshFilter), typeof(MeshRenderer));
        _implicitSurface.hideFlags = HideFlags.HideAndDontSave;
        _implicitSurface.transform.parent = transform;
        _implicitSurface.transform.rotation = Quaternion.identity;
        _implicitSurface.transform.localPosition = Vector3.zero;
        _implicitSurface.transform.localScale = Vector3.one;
        MeshFilter meshFilter = _implicitSurface.GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        Renderer meshRenderer = _implicitSurface.GetComponent<MeshRenderer>();
        meshRenderer.material = material;

        ImplicitSurfaceAnimator animator = GetComponent<ImplicitSurfaceAnimator>();
        if (animator != null)
        {
            animator.Attach(_implicitSurface).Play();
        }
        
        _sdfList.Clear();
        _sdfList = null;
        
        stopwatch.Stop();
        Debug.Log($"{gameObject.name} Implicit Surface Polygonized ({stopwatch.ElapsedMilliseconds}ms)");
    }

    public void Remove()
    {
        if (_implicitSurface != null)
        {
            if (Application.isPlaying)
            {
                Destroy(_implicitSurface);
            }
            else
            {
                DestroyImmediate(_implicitSurface);
            }
            _implicitSurface = null;
        }
    }

    public void EnableRefresh(float seconds = 1f)
    {
        _refreshCoroutine = StartCoroutine(Refresh(seconds));
    }

    public void DisableRefresh()
    {
        if (_refreshCoroutine != null)
        {
            StopCoroutine(_refreshCoroutine);
            _refreshCoroutine = null;
        }
        Remove();
    }

    public IEnumerator Refresh(float seconds)
    {
        while (isActiveAndEnabled)
        {
            Polygonize();
            yield return new WaitForSeconds(seconds);
        }
    }

    private void FetchSDF(GameObject root)
    {
        if (root == null || root.activeSelf == false)
        {
            return;
        }
        
        var implicitSurface = root.GetComponent<ImplicitSurface>();
        if (implicitSurface != null && implicitSurface != this)
        {
            return;
        }
        
        ImplicitSurfaceInfo info = root.GetComponent<ImplicitSurfaceInfo>();
        if (info != null)
        {
            SDFMode sdfMode = info.GetSDFMode();
            Debug.Log($"{sdfMode} Implicit Surface {root.name} : {info.surfaceType}");
            if (sdfMode != SDFMode.None)
            {
                _sdfList.Add((info.GetSDF(), sdfMode));
            }
        }
        
        foreach (Transform child in root.transform)
        {
            FetchSDF(child.gameObject);
        }
    }

    private float SDF(Vector3 position)
    {
        position = transform.TransformPoint(position);
        
        float distance = float.PositiveInfinity;

        foreach ((var sdf, var mode) in _sdfList)
        {
            switch (mode)
            {
                case SDFMode.Add:
                    distance = SmoothAdd(distance, sdf(position), smoothness);
                    break;
                case SDFMode.Subtract:
                    distance = SmoothSubtract(distance, sdf(position), smoothness);
                    break;
            }
        }

        return distance;
    }

    private float SmoothAdd(float a, float b, float k)
    {
        if (a >= float.PositiveInfinity)
        {
            return b;
        }

        if (b >= float.PositiveInfinity)
        {
            return a;
        }
    
        float h = Mathf.Clamp01(0.5f + 0.5f * (b - a) / k);
        return Mathf.Lerp(b, a, h) - k * h * (1.0f - h);
    }

    private float SmoothSubtract(float a, float b, float k)
    {
        float h = Mathf.Clamp01(0.5f - 0.5f * (b + a) / k);
        return Mathf.Lerp(a, -b, h) + k * h * (1.0f - h);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.DrawWireCube(Vector3.zero, 2 * bound);
        Gizmos.matrix = Matrix4x4.identity;
    }

    // private void Start()
    // {
    //     Polygonize();
    // }

    private void OnDestroy()
    {
        DisableRefresh();
    }
}
