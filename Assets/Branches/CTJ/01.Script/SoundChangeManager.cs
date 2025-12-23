using UnityEngine;

public class SoundChangeManager : MonoBehaviour
{
    [SerializeField] AudioClip buildBgm;
    [SerializeField] AudioClip snowBgm;
    AudioSource _as;
    bool _isPlayingBuild = true;

    private void Awake()
    {
        _as = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _as.resource = buildBgm;
        _as.Play();
    }

    public void Inter()
    {
        if(_isPlayingBuild)
        {
            _as.resource = snowBgm;
            _as.Stop();
            _as.Play();
            _isPlayingBuild = false;
        }
        else
        {
            _as.resource = buildBgm;
            _as.Stop();
            _as.Play();
            _isPlayingBuild = true;
        }
    }
}
