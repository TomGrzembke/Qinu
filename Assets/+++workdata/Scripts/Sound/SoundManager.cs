using UnityEngine;

public class SoundManager : MonoBehaviour
{
    #region serialized fields
    public static SoundManager Instance;
    [SerializeField] SoundBankSO soundBank;
    [SerializeField] AudioSource globalMusicSource;
    [SerializeField] AudioSource globalSFXSource;
    #endregion

    #region private fields
    SoundTypeSO[] SoundTypes => soundBank.soundTypes;
    #endregion
    void Awake()
    {
        Instance = this;
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

}

public enum SoundType
{
    Null,
    ButtonHover,
    ButtonClick,
    ButtonClickConfirm,
    ButtonClickBack,

    Bubble,
    SkillAcquired,
    Stun,
    PointCounter,
    PointCounterDown,
    OnSfxChanged
}