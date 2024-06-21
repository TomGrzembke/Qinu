using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharSwitch : MonoBehaviour
{
    #region serialized fields
    //[SerializeField] TextMeshProUGUI leftScoreTxt;
    [SerializeField] TextMeshProUGUI rightScoreTxt;
    [SerializeField] SpriteRenderer rightArenaSR;
    [SerializeField] SpriteRenderer arenaMiddle;
    [SerializeField] float colorBlendTime = 2;
    #endregion

    #region private fields
    CharAestheticSettings rightCharAestheticSettings;
    #endregion

    public void Calculate(List<GameObject> rightPlayers)
    {
        rightCharAestheticSettings = rightPlayers[0].GetComponent<CharSOHolder>().CharSO.charAestheticSettings;

        Color primaryCol = rightCharAestheticSettings.PrimaryColor;
        StartCoroutine(BlendColor(rightScoreTxt, primaryCol));
        StartCoroutine(BlendColor(rightArenaSR, primaryCol));
        StartCoroutine(BlendColor(arenaMiddle, primaryCol));
        SoundManager.Instance.PlayMusic(rightCharAestheticSettings.Music);
    }

    IEnumerator BlendColor(SpriteRenderer target, Color newCol)
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
    IEnumerator BlendColor(TextMeshProUGUI target, Color newCol)
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