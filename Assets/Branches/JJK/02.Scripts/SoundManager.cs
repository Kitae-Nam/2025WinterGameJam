using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoSingleton<SoundManager>
{
    [SerializeField] private AudioMixer audioMixer;
    private AudioSource _audioSource;

    private Dictionary<Sound, AudioClip> soundClipDictionary = new();

    private float sfxVolume = 1f;
    private float bgmVolume = 1f;

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
        bgmVolume = value;
        SetMixerVolume("BGM_Volume", bgmVolume);
        PlayerPrefs.SetFloat("BGM_Volume", bgmVolume);
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = value;
        SetMixerVolume("SFX_Volume", sfxVolume);
        PlayerPrefs.SetFloat("SFX_Volume", sfxVolume);
    }
    
    public float GetBgmVolume() => bgmVolume;
    public float GetSfxVolume() => sfxVolume;
    
    private void SetMixerVolume(string param, float value)
    {
        float db = value <= 0.0001f
            ? -80f
            : Mathf.Log10(value) * 20f;

        audioMixer.SetFloat(param, db);
    }

    private void LoadVolume()
    {
        float bgm = PlayerPrefs.GetFloat("BGM_Volume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFX_Volume", 1f);
        
        SetMixerVolume("BGM_Volume", bgm);
        SetMixerVolume("SFX_Volume", sfx);
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
