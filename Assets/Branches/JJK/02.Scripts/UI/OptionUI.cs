using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;

public class OptionUI : MonoBehaviour
{
    [SerializeField] private GameObject optionUI;
    
    [Header("Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    
    [Header("Icons")]
    [SerializeField] private Image bgmIcon;
    [SerializeField] private Image sfxIcon;
    [SerializeField] private Sprite bgmOnSprite;
    [SerializeField] private Sprite bgmMuteSprite;
    [SerializeField] private Sprite sfxOnSprite;
    [SerializeField] private Sprite sfxMuteSprite;

    private void Start()
    {
        bgmSlider.SetValueWithoutNotify(SoundManager.Instance.GetBgmVolume());
        sfxSlider.SetValueWithoutNotify(SoundManager.Instance.GetSfxVolume());
        
        UpdateIcon();
        
        bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
    }
    
    public void OnClickNext()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    private void OnBgmSliderChanged(float value)
    {
        SoundManager.Instance.SetBgmVolume(value);
        UpdateIcon();
        
        if (bgmSlider.value == 0f)
            ToggleBgm();
    }

    private void OnSfxSliderChanged(float value)
    {
        SoundManager.Instance.SetSfxVolume(value);
        UpdateIcon();
        
        if (sfxSlider.value == 0f)
            ToggleSfx();
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            ToggleUI();
    }

    public void ToggleBgm()
    {
        SoundManager.Instance.BgmMute();
        UpdateIcon();
    }

    public void ToggleSfx()
    {
        
        SoundManager.Instance.SfxMute();
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        bgmIcon.sprite = SoundManager.Instance.IsBgmMuted()
            ? bgmMuteSprite
            : bgmOnSprite;

        sfxIcon.sprite = SoundManager.Instance.IsSfxMuted()
            ? sfxMuteSprite
            : sfxOnSprite;
    }
    
    public void ToggleUI()
    {
        bool isOpen = !optionUI.activeSelf;
        optionUI.SetActive(isOpen);
        Time.timeScale = isOpen ? 0 : 1;

        if (isOpen)
        {
            bgmSlider.SetValueWithoutNotify(SoundManager.Instance.GetBgmVolume());
            sfxSlider.SetValueWithoutNotify(SoundManager.Instance.GetSfxVolume());
            UpdateIcon();
        }
    }

    public void OnClickAgain()
    {
        SoundManager.Instance.PlaySound(Sound.UIClick);
        SceneManager.LoadScene(1);
    }

    public void OnClickHome()
    {
        SoundManager.Instance.PlaySound(Sound.UIClick);
        SceneManager.LoadScene(0);
    }
}
