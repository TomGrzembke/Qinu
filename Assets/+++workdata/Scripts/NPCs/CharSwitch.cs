using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary> Switches the color of the given assets on character change </summary>
public class CharSwitch : MonoBehaviour
{
    #region Serialized
    [SerializeField] TextMeshProUGUI rightScoreTxt;
    [SerializeField] SpriteRenderer rightArenaSR;
    [SerializeField] SpriteRenderer arenaMiddle;
    [SerializeField] float colorBlendTime = 2;
    #endregion

    #region Non Serialized
    CharAestheticSettings rightCharAestheticSettings;
    #endregion

    public void BlendColors(List<GameObject> rightPlayers)
    {
        rightCharAestheticSettings = rightPlayers[0].GetComponent<CharSOHolder>().CharSO.charAestheticSettings;

        Color primaryCol = rightCharAestheticSettings.PrimaryColor;
        StartCoroutine(BlendColorCor(rightScoreTxt, primaryCol));
        StartCoroutine(BlendColorCor(rightArenaSR, primaryCol));
        StartCoroutine(BlendColorCor(arenaMiddle, primaryCol));
        SoundManager.Instance.PlayMusic(rightCharAestheticSettings.Music);
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