using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HelicopterConfig", menuName = "HelicopterConfig")]
public class HelicopterConfig : ScriptableObject
{
    public GameObject prefab;
    public GameObject[] mainWeaponPrefab;
    public GameObject[] frontWeaponPrefab;
    public Vector2 scaleRange;
}