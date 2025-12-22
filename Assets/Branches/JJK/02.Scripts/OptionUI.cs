using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class OptionUI : MonoBehaviour
{
    [SerializeField] private GameObject optionUI;
    
    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            ToggleUI();
    }

    public void ToggleUI()
    {
        optionUI.SetActive(!optionUI.activeSelf);
        Time.timeScale = optionUI.activeSelf ? 0 : 1;
    }
}
