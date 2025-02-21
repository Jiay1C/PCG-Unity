using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }
    
    private void Update()
    {
        CameraInput();
        FlockInput();
        SceneInput();
    }

    private void CameraInput()
    {
        if (Input.GetKeyUp(KeyCode.F))
        {
            CameraController.Instance?.ToggleFirstPerson();
        }

        float dx = Input.GetAxis("Horizontal");
        float dz = Input.GetAxis("Vertical");
        CameraController.Instance?.Move(dx, dz);
    }

    private void FlockInput()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            FlockSystem.Instance?.Run();
        }
    }

    private void SceneInput()
    {
        if (Input.GetKeyUp(KeyCode.Alpha0))
        {
            SceneSwitcher.Main();
        }
        
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            SceneSwitcher.Project1();
        }
        
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            SceneSwitcher.Project2();
        }
        
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            SceneSwitcher.Project3();
        }
        
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            SceneSwitcher.Project4();
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            SceneSwitcher.Exit();
        }
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
