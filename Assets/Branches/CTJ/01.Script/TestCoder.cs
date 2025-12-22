using UnityEngine;
using UnityEngine.InputSystem;

public class TestCoder : MonoBehaviour
{
    [SerializeField] GameObject prefab;

    private void Update()
    {
        if(Keyboard.current.tKey.wasPressedThisFrame)
        {
            Instantiate(prefab, (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition), Quaternion.identity);
        }
    }
}
