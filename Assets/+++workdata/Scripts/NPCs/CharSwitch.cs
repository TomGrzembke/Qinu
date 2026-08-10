using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary> Switches the color of the given assets on character change </summary>
public class CharSwitch : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] secondaryColorSpriteRenderes;
    [SerializeField] TextMeshProUGUI[] secondaryColorTexts;
    [SerializeField] float colorBlendTime = 2;

    CharAestheticSettings rightCharAestheticSettings;

    public void BlendColors(List<GameObject> rightPlayers)
    {
        rightCharAestheticSettings = rightPlayers[0].GetComponent<CharSOHolder>().CharSO.charAestheticSettings;

        Color secondaryArenaColor = rightCharAestheticSettings.PrimaryColor;

        foreach (var entry in secondaryColorSpriteRenderes)
        {
            StartCoroutine(BlendColorCor(entry, secondaryArenaColor));
        }

        foreach (var entry in secondaryColorTexts)
        {
            StartCoroutine(BlendColorCor(entry, secondaryArenaColor));
        }

        SoundManager.Instance.PlayMusic(rightCharAestheticSettings.Music, rightCharAestheticSettings.MusicBlendTime);
    }

    IEnumerator BlendColorCor(SpriteRenderer target, Color newCol)
    {
        float timeWentBy = 0;
        Color oldCol = target.color;

        while (timeWentBy < colorBlendTime)
        {
            timeWentBy += Time.deltaTime;
            target.color = Color.Lerp(oldCol, newCol, timeWentBy / colorBlendTime);
            yield return null;
        }
    }
    IEnumerator BlendColorCor(TextMeshProUGUI target, Color newCol)
    {
        float timeWentBy = 0;
        Color oldCol = target.color;

        while (timeWentBy < colorBlendTime)
        {
            timeWentBy += Time.deltaTime;
            target.color = Color.Lerp(oldCol, newCol, timeWentBy / colorBlendTime);
            yield return null;
        }
    }
}