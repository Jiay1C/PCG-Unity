using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneSwitcher
{
    private static int _nextScene = 0;
    
    private static string _loadingSceneName = "Loading";
    
    private static string[] _sceneName = new[]
    {
        "Main Scene",
        "Project 1",
        "Project 2",
        "Project 3",
        "Project 4",
    };
    
    public static void Exit()
    {
        Application.Quit();
    }

    public static void Main()
    {
        _nextScene = 0;
        SceneManager.LoadScene(_loadingSceneName);
    }

    public static void Project1()
    {
        _nextScene = 1;
        SceneManager.LoadScene(_loadingSceneName);
    }

    public static void Project2()
    {
        _nextScene = 2;
        SceneManager.LoadScene(_loadingSceneName);
    }

    public static void Project3()
    {
        _nextScene = 3;
        SceneManager.LoadScene(_loadingSceneName);
    }

    public static void Project4()
    {
        _nextScene = 4;
        SceneManager.LoadScene(_loadingSceneName);
    }

    public static void Switch()
    {
        if (_nextScene < 0)
        {
            Debug.Log("Unspecified Next Scene");
            return;
        }
        string sceneName = _sceneName[_nextScene];
        Debug.Log($"Switch To {sceneName}");
        _nextScene = -1;
        SceneManager.LoadScene(sceneName);
    }
}
