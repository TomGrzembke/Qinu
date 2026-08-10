using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider textSpeedSlider;
    [SerializeField] TMP_Text textSpeedPreview;
    [SerializeField] Toggle screenToggle;
    [SerializeField] float onSFXChangedCooldown = 0.1f;

    Coroutine sfxChangedCoroutine;
    Coroutine textSpeedPreviewCoroutine;
    bool sfxEmitSound;

    const string MUSIC_VOLUME_KEY = "musicVolume";
    const string SFX_VOLUME_KEY = "sfxVolume";
    const string TEXT_SPEED_MULTIPLIER_KEY = "textSpeedMultiplier";
    const float DEFAULT_TEXT_SPEED_MULTIPLIER = 1f;

    void OnEnable()
    {
        musicSlider.value = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY);
        sfxSlider.value = PlayerPrefs.GetFloat(SFX_VOLUME_KEY);

        if (textSpeedSlider != null)
        {
            textSpeedSlider.SetValueWithoutNotify(GetTextSpeedMultiplier());
        }

        if (textSpeedPreview != null)
        {
            textSpeedSlider ??= textSpeedPreview.GetComponentInParent<Slider>();
            textSpeedSlider?.SetValueWithoutNotify(GetTextSpeedMultiplier());
            RestartTextSpeedPreview();
        }

        GetScreenToggle();
        InputManager.Instance.ShowCursor();
    }

    public void OnMusicSliderChanged()
    {
        float volume = musicSlider.value;

        if (volume == musicSlider.minValue)
        {
            volume = -60;
        }   

        audioMixer.SetFloat("musicVolume", volume);
        PlayerPrefs.SetFloat("musicVolume", volume);
        musicSlider.value = volume;
    }

    public void OnSfxSliderChanged()
    {
        float volume = sfxSlider.value;

        if (volume == sfxSlider.minValue)
        {
            volume = -60;
        }

        audioMixer.SetFloat("sfxVolume", volume);
        PlayerPrefs.SetFloat("sfxVolume", volume);
        sfxSlider.value = volume;

        if (sfxChangedCoroutine == null && sfxEmitSound)
        {
            sfxChangedCoroutine = StartCoroutine(PlayOnSFXChangedCor());
        }

        sfxEmitSound = true;
    }

    public void OnTextSpeedSliderChanged(float multiplier)
    {
        PlayerPrefs.SetFloat(TEXT_SPEED_MULTIPLIER_KEY, multiplier);

        if (DialogueController.Instance != null)
        {
            DialogueController.Instance.SetTextSpeedMultiplier(multiplier);
        }

        RestartTextSpeedPreview();
    }

    public static float GetTextSpeedMultiplier()
    {
        return PlayerPrefs.GetFloat(TEXT_SPEED_MULTIPLIER_KEY, DEFAULT_TEXT_SPEED_MULTIPLIER);
    }

    void RestartTextSpeedPreview()
    {
        if (textSpeedPreview == null || !isActiveAndEnabled) return;

        if (textSpeedPreviewCoroutine != null)
        {
            StopCoroutine(textSpeedPreviewCoroutine);
        }

        textSpeedPreviewCoroutine = StartCoroutine(PlayTextSpeedPreview());
    }

    IEnumerator PlayTextSpeedPreview()
    {
        string previewText = textSpeedPreview.text;

        while (true)
        {
            textSpeedPreview.maxVisibleCharacters = 0;
            float characterDelay = DialogueController.GetTypeDelay(GetTextSpeedMultiplier());

            for (int visibleCharacters = 1; visibleCharacters <= previewText.Length; visibleCharacters++)
            {
                textSpeedPreview.maxVisibleCharacters = visibleCharacters;
                yield return new WaitForSecondsRealtime(characterDelay);
            }

            yield return new WaitForSecondsRealtime(0.75f);
        }
    }

    void GetScreenToggle()
    {
        if (screenToggle == null) return;

        screenToggle.isOn = PlayerPrefs.GetInt("fullscreenID") == 0;
        Screen.fullScreen = screenToggle.isOn;
    }

    public void FullScreenToggle()
    {
        if (screenToggle == null) return;

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
