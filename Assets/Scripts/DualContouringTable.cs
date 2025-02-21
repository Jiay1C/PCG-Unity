using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DualContouring
{
    private static readonly Vector3Int[] _cellPoint =
    {
        new (0, 0, 0),
        new (1, 0, 0),
        new (1, 0, 1),
        new (0, 0, 1),
        new (0, 1, 0),
        new (1, 1, 0),
        new (1, 1, 1),
        new (0, 1, 1),
    };
    
    private static readonly Vector2Int[] _cellEdge =
    {
        new (0, 1),
        new (1, 2),
        new (2, 3),
        new (3, 0),
        new (4, 5),
        new (5, 6),
        new (6, 7),
        new (7, 4),
        new (0, 4),
        new (1, 5),
        new (2, 6),
        new (3, 7),
    };

    private static readonly (Vector3Int, Vector3Int[])[] _pointOffset =
    {
        (new(1, 0, 0), new []
        {
            new Vector3Int(0, -1, -1),
            new Vector3Int(0, 0, -1),
            new Vector3Int(0, -1, 0),
            new Vector3Int(0, 0, 0),
        }),
        (new(0, 1, 0), new []
        {
            new Vector3Int(-1, 0, -1),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, -1),
            new Vector3Int(0, 0, 0),
        }),
        (new(0, 0, 1), new []
        {
            new Vector3Int(-1, -1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, 0),
        }),
    };
    
}
