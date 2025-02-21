using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    
    public float translateSensitivity = 0.3f;
    public float rotateSensitivity = 5.0f;
    public float firstPersonHeight = 2f;
    
    public Vector3 CameraPosition { get { return _mainCameraTransform.position; } }
    public Quaternion CameraRotation { get { return _mainCameraTransform.rotation; } }
    public Vector3 CameraForward { get { return _mainCameraTransform.forward; } }
    
    private Camera _mainCamera;
    private bool _firstPerson;
    private Transform _mainCameraTransform;
    private float _mainCameraInitHeight;

    public void Move(float dx, float dz)
    {
        // move the camera based on keyboard input
        // translate forward or backwards
        _mainCameraTransform.Translate(0, 0, dz * translateSensitivity * Time.deltaTime * 100);

        // rotate left or right
        _mainCameraTransform.Rotate(0, dx * rotateSensitivity * Time.deltaTime * 100, 0);
        
        if (_firstPerson)
        {
            Vector3 originPos = _mainCameraTransform.position;
            originPos.y = _mainCameraInitHeight;
            if (Physics.Raycast(originPos, Vector3.down, out var hit))
            {
                Vector3 pos = _mainCameraTransform.position;
                pos.y = hit.point.y + firstPersonHeight;
                _mainCameraTransform.position = pos;
            }
        }
    }

    public void ToggleFirstPerson()
    {
        _firstPerson = !_firstPerson;
        if (!_firstPerson)
        {
            Vector3 pos = _mainCameraTransform.position;
            pos.y = _mainCameraInitHeight;
            _mainCameraTransform.position = pos;
            Debug.Log("<color=green>Switch to fly camera</color>");
        }
        else
        {
            Debug.Log("<color=green>Switch to first person camera</color>");
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
        
        _mainCamera = Camera.main;
        _mainCameraTransform = _mainCamera.transform;
        _mainCameraInitHeight = _mainCameraTransform.position.y;
    }
}
