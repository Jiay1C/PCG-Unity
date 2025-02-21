using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImplicitSurfaceManager : MonoBehaviour
{
    public void Polygonize()
    {
        foreach (ImplicitSurface surface in GetComponentsInChildren<ImplicitSurface>())
        {
            surface.Polygonize();
        }
    }
    
    public void Remove()
    {
        foreach (ImplicitSurface surface in GetComponentsInChildren<ImplicitSurface>())
        {
            surface.Remove();
        }
    }

    public void EnableRefresh(float seconds = 1)
    {
        foreach (ImplicitSurface surface in GetComponentsInChildren<ImplicitSurface>())
        {
            surface.EnableRefresh(seconds);
        }
    }
    
    public void DisableRefresh()
    {
        foreach (ImplicitSurface surface in GetComponentsInChildren<ImplicitSurface>())
        {
            surface.DisableRefresh();
        }
    }
}