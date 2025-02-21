using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LeafConfig", menuName = "LeafConfig")]
public class LeafConfig : ScriptableObject
{
    public int verticesCount = 10;
    public Vector2 size = new Vector2(0.1f, 0.2f);
    public Vector2 width = new Vector2(0.4f, 0.6f);
    public Vector2 center = new Vector2(0.4f, 0.6f);
}
