using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] SoundBankSO soundBank;
    [SerializeField] AudioSource globalMusicSource;
    [SerializeField] AudioSource globalSFXSource;
    [SerializeField] DialogueSoundPlayer soundPlayer;
    [Header("Music")]
    [SerializeField] float musicBlendTime;

    public static SoundManager Instance;
    SoundTypeSO[] SoundTypes => soundBank.soundTypes;
    Coroutine musicRoutine;
    float originalMusicVolume;

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

        if (clip == null) return;

        if (localSource == null || !localSource.gameObject.activeInHierarchy)
        {
            globalSFXSource.PlayOneShot(clip);
            return;
        }

        localSource.PlayOneShot(clip);
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

        if (clip == null) return 0;

        return clip.length;
    }

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

    public void PlayMusic(AudioClip clip, float musicBlendTimeOverride = -1)
    {
        if (clip == globalMusicSource.clip) return;

        if (musicRoutine != null)
        {
            StopCoroutine(musicRoutine);
        }

        musicRoutine = StartCoroutine(BlendMusic(clip, musicBlendTimeOverride));
    }

    IEnumerator BlendMusic(AudioClip clip, float musicBlendTimeOverride = -1)
    {
        float blendTime = Mathf.Approximately(-1, musicBlendTimeOverride) ? musicBlendTime : musicBlendTimeOverride;
        float timeWentBy = 0;

        while (timeWentBy < blendTime)
        {
            timeWentBy += Time.deltaTime;
            globalMusicSource.volume = Mathf.Lerp(originalMusicVolume, 0, timeWentBy / blendTime);
            yield return null;
        }

        globalMusicSource.volume = 0;
        globalMusicSource.clip = clip;
        globalMusicSource.Play();
        timeWentBy = 0;

        while (timeWentBy < blendTime)
        {
            timeWentBy += Time.deltaTime;
            globalMusicSource.volume = Mathf.Lerp(0, originalMusicVolume, timeWentBy / blendTime);
            yield return null;
        }
    }
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
    AbilityCooldown = 12,
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