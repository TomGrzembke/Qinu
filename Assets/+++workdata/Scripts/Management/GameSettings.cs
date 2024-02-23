using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    #region Serilized Fields
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider musicSlider, sfxSlider;
    [SerializeField] Toggle screenToggle;
    [SerializeField] float onSFXChangedCooldown = 0.1f;
    #endregion

    #region private
    Coroutine sfxChangedCoroutine;
    bool sfxEmitSound;
    #endregion

    void Start()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");

        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");

        GetScreenToggle();
    }

    public void OnMusicSliderChanged()
    {
        float volume = musicSlider.value;

        if (volume == musicSlider.minValue)
            volume = -60;

        audioMixer.SetFloat("musicVolume", volume);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void OnSfxSliderChanged()
    {
        float volume = sfxSlider.value;

        if (volume == sfxSlider.minValue)
            volume = -60;

        audioMixer.SetFloat("sfxVolume", volume);
        PlayerPrefs.SetFloat("sfxVolume", volume);
        sfxSlider.value = volume;

        if (sfxChangedCoroutine == null && sfxEmitSound)
            sfxChangedCoroutine = StartCoroutine(PlayOnSFXChangedCor());
        sfxEmitSound = true;
    }

    void GetScreenToggle()
    {
        screenToggle.isOn = PlayerPrefs.GetInt("fullscreenID") == 0;
        Screen.fullScreen = screenToggle.isOn;
    }

    public void FullScreenToggle()
    {
        bool isFullscreen = screenToggle.isOn;
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("fullscreenID", (isFullscreen ? 0 : 1));
    }

    public void OpenURL(string link)
    {
        Application.OpenURL(link);
    }

    IEnumerator PlayOnSFXChangedCor()
    {
        SoundManager.Instance.PlaySound(SoundType.OnSfxChanged);
        yield return new WaitForSecondsRealtime(onSFXChangedCooldown);
        sfxChangedCoroutine = null;
    }
}
