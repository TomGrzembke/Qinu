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
            ChangeCharSO(defaultCharSO);
            PlaySpecialParticles(false);
            yield return new WaitForSeconds(timeInDefault);
            ChangeCharSO(specialCharSO);
            PlaySpecialParticles(true);
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

    void ChangeCharSO(NPCCharSO charSO)
    {
        charSOHolder.ChangeCharSO(charSO);
        SoundManager.Instance.PlayMusic(charSO.charAestheticSettings.Music, musicBlendTimeOverride);
    }

    void StopSwitching()
    {
        StopCoroutine(switchRoutine);
        ChangeCharSO(defaultCharSO);
    }
}
