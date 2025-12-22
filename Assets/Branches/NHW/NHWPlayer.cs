using UnityEngine;
using UnityEngine.InputSystem;

public class NHWPlayer : MonoBehaviour
{
    public camera _cameraController;
    private void Start()
    {
            _cameraController.SwitchTo(0);

    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {  Debug.Log("Switching to camera 1");
            _cameraController.SwitchTo(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Switching to camera 2");
            _cameraController.SwitchTo(1);
        }
        if (Keyboard.current.aKey.isPressed)
        {
            Debug.Log("A");
            _cameraController.MoveCamera(1);
        }
        if (Keyboard.current.dKey.isPressed)
        {
            Debug.Log("D");
            _cameraController.MoveCamera(2);

        }
    }
}
