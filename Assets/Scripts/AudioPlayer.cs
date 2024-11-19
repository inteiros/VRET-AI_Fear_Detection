using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;

    public AudioClip chill;
    public AudioClip underwater;
    public AudioClip sea;
    void Start()
    {
        audioSource.loop = true;
        audioSource.volume = 0.5f;
    }

    public void PlayChill()
    {
        audioSource.clip = chill;
        audioSource.loop = true;
        audioSource.volume = 0.5f;
        audioSource.Play();
    }
    public void PlayUnderwater()
    {
        audioSource.clip = underwater;
        audioSource.loop = true;
        audioSource.volume = 0.5f;
        audioSource.Play();
    }

    public void PlaySea()
    {
        audioSource.clip = sea;
        audioSource.loop = true;
        audioSource.volume = 0.5f;
        audioSource.Play();
    }
    public void StopSFX()
    {
        audioSource.Stop();
    }
}
