using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;
    public AudioClip MenuTrack;
    public AudioClip BackgroundTrack;
    public AudioClip GameOverTrack;
    public AudioClip Death;
    public AudioClip DeadEnemy;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
    }
}


