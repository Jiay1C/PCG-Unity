using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ImplicitSurface))]
public class ImplicitSurfaceDebugger : Editor
{
    private string refreshSecondsString = "0.1";
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ImplicitSurface implicitSurface = (ImplicitSurface)target;
        
        EditorGUILayout.Space();

        refreshSecondsString = GUILayout.TextArea(refreshSecondsString, EditorStyles.textArea);
        
        
        
        if (GUILayout.Button("Polygonize"))
        {
            implicitSurface.Polygonize();
        }

        if (GUILayout.Button("Remove"))
        {
            implicitSurface.Remove();
        }
        
        if (GUILayout.Button("Enable Refresh"))
        {
            if (float.TryParse(refreshSecondsString, out var refreshSecondsFloat))
            {
                implicitSurface.EnableRefresh(refreshSecondsFloat);
            }
            else
            {
                Debug.LogError($"Invalid Refresh Seconds: {refreshSecondsString}");
            }
        }
        
        if (GUILayout.Button("Disable Refresh"))
        {
            implicitSurface.DisableRefresh();
        }
    }
}

[CustomEditor(typeof(ImplicitSurfaceManager))]
public class ImplicitSurfaceManagerDebugger : Editor
{
    private string refreshSecondsString = "0.1";
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ImplicitSurfaceManager implicitSurface = (ImplicitSurfaceManager)target;
        
        EditorGUILayout.Space();

        refreshSecondsString = GUILayout.TextArea(refreshSecondsString, EditorStyles.textArea);
        
        
        
        if (GUILayout.Button("Polygonize"))
        {
            implicitSurface.Polygonize();
        }

        if (GUILayout.Button("Remove"))
        {
            implicitSurface.Remove();
        }
        
        if (GUILayout.Button("Enable Refresh"))
        {
            if (float.TryParse(refreshSecondsString, out var refreshSecondsFloat))
            {
                implicitSurface.EnableRefresh(refreshSecondsFloat);
            }
            else
            {
                Debug.LogError($"Invalid Refresh Seconds: {refreshSecondsString}");
            }
        }
        
        if (GUILayout.Button("Disable Refresh"))
        {
            implicitSurface.DisableRefresh();
        }
    }
}