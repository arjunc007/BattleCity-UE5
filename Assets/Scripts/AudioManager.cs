using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource _sfxPlayer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void PlayOneShot(AudioClip clip)
    {
        _sfxPlayer.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip)
    {
        _sfxPlayer.clip = clip;
        _sfxPlayer.Play();
    }

    public void Stop()
    {
        _sfxPlayer.Stop();
    }

    public bool IsPlaying(AudioClip clip)
    {
        return _sfxPlayer.clip == clip && _sfxPlayer.isPlaying;
    }
}
