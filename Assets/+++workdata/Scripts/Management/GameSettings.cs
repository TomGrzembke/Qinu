using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    #region Serilized Fields
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider masterSlider;
    [SerializeField] Toggle screenToggle;
    [SerializeField] float onSFXChangedCooldown = 0.1f;
    #endregion

    #region private
    Coroutine sfxChangedCoroutine;
    bool sfxEmitSound;
    #endregion

    void Start()
    {
        masterSlider.value = PlayerPrefs.GetFloat("masterVolume");

        GetScreenToggle();
    }

    public void OnMasterSliderChanged()
    {
        float volume = masterSlider.value;

        if (volume == masterSlider.minValue)
            volume = -60;

        audioMixer.SetFloat("masterVolume", volume);
        PlayerPrefs.SetFloat("masterVolume", volume);
        masterSlider.value = volume;

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
        //SoundManager.Instance.PlaySound(SoundType.OnSfxChanged);
        yield return new WaitForSecondsRealtime(onSFXChangedCooldown);
        sfxChangedCoroutine = null;
    }
}
