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
    #endregion

    #region private fields
    CharAestheticSettings rightCharAestheticSettings;
    #endregion

    public void Calculate(List<GameObject> rightPlayers)
    {
        rightCharAestheticSettings = rightPlayers[0].GetComponent<CharSOHolder>().CharSO.charAestheticSettings;

        rightScoreTxt.color = rightCharAestheticSettings.PrimaryColor;
        rightArenaSR.color = rightCharAestheticSettings.PrimaryColor;
        arenaMiddle.color = rightCharAestheticSettings.PrimaryColor;

    }

}