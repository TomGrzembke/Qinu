using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public enum Songs
    {
        MainMenu,
        Ingame,
        Sunfish
    }
    #region serialized fields
    [SerializeField] float fadeTime = 3;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip mainMenuClip;
    [SerializeField] AudioClip ingameClip;
    [SerializeField] AudioClip sunfishClip;
    public static MusicManager Instance;
    #endregion

    #region private fields
    AudioClip playedLast;
    Coroutine fadeRoutine;
    #endregion

    void Awake()
    {
        Instance = this;
    }

    public void PlaySong(Songs song)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        AudioClip contextClip = song switch
        {
            Songs.MainMenu => mainMenuClip,
            Songs.Ingame => ingameClip,
            Songs.Sunfish => sunfishClip,
            _ => ingameClip,
        };

        fadeRoutine = StartCoroutine(FadeSong(contextClip));
    }

    public void PlayLastPlayed()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeSong(playedLast));
    }

    IEnumerator FadeSong(AudioClip clip)
    {
        float fadedTime = 0;
        float currentVolume = audioSource.volume;

        while (fadedTime < fadeTime / 2)
        {
            fadedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(currentVolume, 0, fadedTime / fadeTime * 2);
            yield return null;
        }

        playedLast = audioSource.clip;
        audioSource.clip = clip;
        audioSource.Play();

        fadedTime = 0;
        while (fadedTime < fadeTime / 2)
        {
            fadedTime += Time.deltaTime;

            audioSource.volume = Mathf.Lerp(0, currentVolume, fadedTime / fadeTime * 2);
            yield return null;
        }
    }
}