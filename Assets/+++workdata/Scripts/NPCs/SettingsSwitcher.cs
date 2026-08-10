using System.Collections;
using UnityEngine;

public class SettingsSwitcher : MonoBehaviour
{
    [SerializeField] CharSOHolder charSOHolder;

    [SerializeField] NPCCharSO defaultCharSO;
    [SerializeField] NPCCharSO specialCharSO;

    [SerializeField] float timeInDefault = 10;
    [SerializeField] float timeInSpecial = 5;
    [SerializeField] float musicBlendTimeOverride = .64f;

    [SerializeField] ParticleSystem[] specialSystems;
    [SerializeField] ParticleSystem[] defaultSystems;
    Coroutine switchRoutine;



    void Start()
    {
        switchRoutine = StartCoroutine(SwitchRoutine());
    }

    IEnumerator SwitchRoutine()
    {
        while (true)
        {
            yield return null;
            ChangeCharSO(defaultCharSO, specialCharSO);
            PlayDefaultParticles(true);
            PlaySpecialParticles(false);
            yield return new WaitForSeconds(timeInDefault);
            ChangeCharSO(specialCharSO, defaultCharSO);
            PlaySpecialParticles(true);
            PlayDefaultParticles(false);

            yield return new WaitForSeconds(timeInSpecial);
        }
    }

    void PlaySpecialParticles(bool condition)
    {
        foreach (var entry in specialSystems)
        {
            if (condition)
            {
                entry.Play();
            }
            else
            {
                entry.Stop();
            }
        }
    }

    void PlayDefaultParticles(bool condition)
    {
        foreach (var entry in defaultSystems)
        {
            if (condition)
            {
                entry.Play();
            }
            else
            {
                entry.Stop();
            }
        }
    }

    void ChangeCharSO(NPCCharSO charSO, NPCCharSO requiredMusicState)
    {
        charSOHolder.ChangeCharSO(charSO);

        if (!SoundManager.Instance.IsMusicPlayingOrRequested(requiredMusicState.charAestheticSettings.Music)) return;

        SoundManager.Instance.PlayMusic(charSO.charAestheticSettings.Music, musicBlendTimeOverride);
    }

    void StopSwitching()
    {
        StopCoroutine(switchRoutine);
        ChangeCharSO(defaultCharSO, specialCharSO);
    }
}
