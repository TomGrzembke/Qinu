using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class SoundManager : MonoBehaviour
{
    #region serialized fields
    public static SoundManager Instance;
    [SerializeField] SoundBankSO soundBank;
    [SerializeField] AudioSource globalMusicSource;
    [SerializeField] AudioSource globalSFXSource;
    [SerializeField] DialogueSoundPlayer soundPlayer;

    [Header("Music")]
    [SerializeField] float musicBlendTime;
    #endregion

    #region private fields
    SoundTypeSO[] SoundTypes => soundBank.soundTypes;
    Coroutine musicRoutine;
    float originalMusicVolume;
    #endregion

    void Awake()
    {
        Instance = this;
        originalMusicVolume = globalMusicSource.volume;
    }

    public void PlayVoice(SoundType type)
    {
        soundPlayer.PlaySound(type);
    }

    public void PlaySound(SoundType type, AudioSource localSource = null)
    {
        AudioClip clip = null;

        for (int i = 0; i < SoundTypes.Length; i++)
        {
            if (SoundTypes[i].soundType != type) continue;

            clip = SoundTypes[i].clips[Random.Range(0, SoundTypes[i].clips.Length)];
            break;
        }

        if (localSource && clip != null)
        {
            if (localSource.gameObject.activeInHierarchy)
                localSource.PlayOneShot(clip);
        }
        else
            globalSFXSource.PlayOneShot(clip);
    }

    /// <summary> Gets the index 0 sound length of given type </summary>
    public float GetSoundLength(SoundType type)
    {
        AudioClip clip = null;

        for (int i = 0; i < SoundTypes.Length; i++)
        {
            if (SoundTypes[i].soundType != type) continue;

            clip = SoundTypes[i].clips[0];
            break;
        }

        if (clip == null)
            return 0;

        return clip.length;
    }

    #region ButtonMethods
    public void PlaySoundButtonClick()
    {
        PlaySound(SoundType.ButtonClick);
    }
    public void PlaySoundButtonHover()
    {
        PlaySound(SoundType.ButtonHover);
    }
    public void PlaySoundButtonClickBack()
    {
        PlaySound(SoundType.ButtonClickBack);
    }
    #endregion

    #region Music
    public void PlayMusic(AudioClip clip)
    {
        if (clip == globalMusicSource.clip) return;

        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(BlendMusic(clip));
    }

    IEnumerator BlendMusic(AudioClip clip)
    {
        float timeWentBy = 0;

        while (timeWentBy < musicBlendTime)
        {
            timeWentBy += Time.deltaTime;
            globalMusicSource.volume = Mathf.Lerp(originalMusicVolume, 0, timeWentBy / musicBlendTime);
            yield return null;
        }

        globalMusicSource.volume = 0;
        globalMusicSource.clip = clip;
        globalMusicSource.Play();
        timeWentBy = 0;

        while (timeWentBy < musicBlendTime)
        {
            timeWentBy += Time.deltaTime;
            globalMusicSource.volume = Mathf.Lerp(0, originalMusicVolume, timeWentBy / musicBlendTime);
            yield return null;
        }
    }
    #endregion
}

public enum SoundType
{
    Null,
    ButtonHover,
    ButtonClick,
    ButtonClickConfirm,
    ButtonClickBack,

    SkillAcquired,
    Stun,
    PointCounter,
    PointCounterDown,
    OnSfxChanged,
    Qinu,
    Anthony,
    Bodi,
    Pamo,
    Tessar,
    Reaf
}