using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loading : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(SwitchScene());
    }

    IEnumerator SwitchScene()
    {
        yield return null;
        SceneSwitcher.Switch();
    }
}
