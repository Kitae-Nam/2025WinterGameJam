using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class NHWPlayer : MonoBehaviour
{
    public camera _cameraController;
    public event Action AAA;
    public Transform Transform;
    public GameObject GameObject;
    Rigidbody2D rb;
    private void Start()
    {
        _cameraController.SwitchTo(0);
        rb = GetComponent<Rigidbody2D>();

        AAA += () => _cameraController.SwitchTo(1);
        AAA += SettingP;
    }
    public void SettingP()
    {
        GameObject.transform.position = Transform.position;
        GameObject.SetActive(true);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Switching to camera 1");
            _cameraController.SwitchTo(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Switching to camera 2");
            AAA?.Invoke();
        }
        if (Keyboard.current.aKey.isPressed)
        {
            Debug.Log("A");
            _cameraController.MoveCamera(1);
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            Debug.Log("D");
            _cameraController.MoveCamera(2);

        }
        else if (Keyboard.current.wKey.isPressed)
        {
            Debug.Log("W");
            _cameraController.MoveCamera(3);
        }
        else if (Keyboard.current.sKey.isPressed)
        {
            Debug.Log("S");
            _cameraController.MoveCamera(4);
        }
    }

    public void StartHandler()
    {
        AAA?.Invoke();
    }
}

