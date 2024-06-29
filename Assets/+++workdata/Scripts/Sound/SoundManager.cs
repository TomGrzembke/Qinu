using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    #region Serialized
    [SerializeField] SoundBankSO soundBank;
    [SerializeField] AudioSource globalMusicSource;
    [SerializeField] AudioSource globalSFXSource;
    [SerializeField] DialogueSoundPlayer soundPlayer;
    [Header("Music")]
    [SerializeField] float musicBlendTime;
    #endregion

    #region Non Serialized
    public static SoundManager Instance;
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
    Null = 0,
    ButtonHover = 1,
    ButtonClick = 2,
    ButtonClickConfirm = 3,
    ButtonClickBack = 4,

    SkillAcquired = 5,
    AbilityPopup = 6,
    Stun = 7,
    PointCounter = 8,
    PointCounterDown = 9,
    OnSfxChanged = 10,
    BallHit = 11,
    Qinu = 13,
    Anthony = 14,
    Bodi = 15,
    Pamo = 16,
    Tessar = 17,
    Reaf = 18,
    GoalShotHard = 19,
    GoalShotMiddle = 20,
    GoalShotSoft = 21,

}