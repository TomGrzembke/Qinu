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
            yield return new WaitForSeconds(timeInDefault);
            ChangeCharSO(specialCharSO);
            yield return new WaitForSeconds(timeInSpecial);
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
