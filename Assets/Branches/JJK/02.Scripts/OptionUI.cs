using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [SerializeField] private GameObject optionUI;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        bgmSlider.value = SoundManager.Instance.GetBgmVolume();
        sfxSlider.value = SoundManager.Instance.GetSfxVolume();
    }

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
