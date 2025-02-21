using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BushConfig", menuName = "BushConfig")]
public class BushConfig : ScriptableObject
{
    public int MaxDegree = 3;
    
    public float BranchRadius = 0.1f;
    public float BranchRadiusRatio = 0.5f;
    public float LineSegment = 0.05f;
    public float CircleSegment = 0.001f;
    public int MinSegment = 4;
    public Vector2 UVRatio = new Vector2(1f, 1f);

    public Vector2[] BranchLength = new[]
    {
        new Vector2(5.0f, 8.0f),
        new Vector2(1f, 2f),
        new Vector2(0.5f, 1f),
    };
    
    public Vector2[] BranchPitchRange = new[]
    {
        new Vector2(0.0f, 10.0f),
        new Vector2(30.0f, 60.0f),
        new Vector2(30.0f, 60.0f),
    };
    
    public Vector2[] BranchDistance = new[]
    {
        new Vector2(1.0f, 1.0f),
        new Vector2(0.05f, 0.16f),
        new Vector2(0.05f, 0.1f),
    };
    
    public Vector2[] BranchSpawnRange = new[]
    {
        new Vector2(0.0f, 1.0f),
        new Vector2(0.2f, 0.8f),
        new Vector2(0.2f, 0.8f),
    };

    public float[] MaxCurve = new[]
    {
        0.2f, 0.1f, 0.1f
    };
    
    public float[] Probability = new[]
    {
        1.0f, 1.0f, 1.0f
    };
    
    public Vector2[] LeafPitchRange = new[]
    {
        new Vector2(0.0f, 10.0f),
        new Vector2(30.0f, 60.0f),
        new Vector2(30.0f, 60.0f),
    };
    
    public Vector2[] LeafDistance = new[]
    {
        new Vector2(1.0f, 1.0f),
        new Vector2(0.05f, 0.16f),
        new Vector2(0.05f, 0.1f),
    };
    
    public Vector2[] LeafSpawnRange = new[]
    {
        new Vector2(0.0f, 1.0f),
        new Vector2(0.2f, 0.8f),
        new Vector2(0.2f, 0.8f),
    };

    public bool Valid()
    {
        return MaxDegree == BranchLength.Length &&
               MaxDegree == BranchPitchRange.Length &&
               MaxDegree == BranchDistance.Length &&
               MaxDegree == BranchSpawnRange.Length &&
               MaxDegree == MaxCurve.Length &&
               MaxDegree == Probability.Length &&
               MaxDegree == LeafPitchRange.Length &&
               MaxDegree == LeafDistance.Length &&
               MaxDegree == LeafSpawnRange.Length;
    }
}
