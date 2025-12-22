using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoSingleton<SoundManager>
{
    [SerializeField] private AudioMixer audioMixer;
    private AudioSource _audioSource;

    private Dictionary<Sound, AudioClip> soundClipDictionary = new();

    private float _sfxVolume = 1f;
    private float _bgmVolume = 1f;
    
    private bool _isBgmMuted;
    private bool _isSfxMuted;
    private float _prevBgmVolume = 1f;
    private float _prevSfxVolume = 1f;

    protected override void Awake()
    {
        base.Awake();
        _audioSource = GetComponent<AudioSource>();

        foreach (Sound c in Enum.GetValues(typeof(Sound)))
        {
            soundClipDictionary[c] = Resources.Load<AudioClip>(c.ToString());
        }
        
        LoadVolume();
    }

    public void PlaySound(Sound sound)
    {
        _audioSource.PlayOneShot(soundClipDictionary[sound]);
    }
    
    public void SetBgmVolume(float value)
    {
        _bgmVolume = value;
        
        if (_isBgmMuted)
        {
            _isBgmMuted = false;
            PlayerPrefs.SetInt("BGM_MUTE", 0);
        }
        
        SetMixerVolume("BGM_Volume", _bgmVolume);
        PlayerPrefs.SetFloat("BGM_Volume", _bgmVolume);
    }

    public void SetSfxVolume(float value)
    {
        _sfxVolume = value;
        
        if (_isSfxMuted)
        {
            _isSfxMuted = false;
            PlayerPrefs.SetInt("SFX_MUTE", 0);
        }
        
        SetMixerVolume("SFX_Volume", _sfxVolume);
        PlayerPrefs.SetFloat("SFX_Volume", _sfxVolume);
    }
    
    public float GetBgmVolume() => _bgmVolume;
    public float GetSfxVolume() => _sfxVolume;
    
    public void BgmMute()
    {
        _isBgmMuted = !_isBgmMuted;

        if (_isBgmMuted)
        {
            _prevBgmVolume = _bgmVolume;
            SetMixerVolume("BGM_Volume", 0f);
        }
        else
        {
            SetMixerVolume("BGM_Volume", _prevBgmVolume);
        }

        PlayerPrefs.SetInt("BGM_MUTE", _isBgmMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public void SfxMute()
    {
        _isSfxMuted = !_isSfxMuted;

        if (_isSfxMuted)
        {
            _prevSfxVolume = _sfxVolume;
            SetMixerVolume("SFX_Volume", 0f);
        }
        else
        {
            SetMixerVolume("SFX_Volume", _prevSfxVolume);
        }

        PlayerPrefs.SetInt("SFX_MUTE", _isSfxMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public bool IsBgmMuted() => _isBgmMuted;
    public bool IsSfxMuted() => _isSfxMuted;
    
    private void SetMixerVolume(string param, float value)
    {
        float db = value <= 0.0001f
            ? -80f
            : Mathf.Log10(value) * 20f;

        audioMixer.SetFloat(param, db);
    }

    private void LoadVolume()
    {
        _bgmVolume = PlayerPrefs.GetFloat("BGM_Volume", 1f);
        _sfxVolume = PlayerPrefs.GetFloat("SFX_Volume", 1f);
        
        SetMixerVolume("BGM_Volume", _bgmVolume);
        SetMixerVolume("SFX_Volume", _sfxVolume);
        
        SetMixerVolume("BGM_Volume", _isBgmMuted ? 0f : _bgmVolume);
        SetMixerVolume("SFX_Volume", _isSfxMuted ? 0f : _sfxVolume);
    }
}

public enum Sound
{
    Clear,
    GameOver,
    Build,
    Run,
    Jump
}
