using UnityEngine;

public class SkiAudioManager : MonoBehaviour
{
    [SerializeField] ParticleSystem skiParticle;
    AudioSource _as;

    private void Awake()
    {
        _as = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!skiParticle) return;

        bool alive = skiParticle.IsAlive(true);

        if (alive && !_as.isPlaying) _as.Play();
        if (!alive && _as.isPlaying) _as.Stop();
    }
}
