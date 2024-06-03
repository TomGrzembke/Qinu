using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WinLooseBar : MonoBehaviour
{
    #region serialized fields
    [SerializeField] Slider bar;
    #endregion

    IEnumerator Start()
    {
        yield return null;
        TournamentManager.Instance.RegisterOnPlayerMatchEnd(OnValueChanged, true);
    }

    void OnDisable()
    {
        TournamentManager.Instance.OnPlayerMatchEnd -= OnValueChanged;
    }

    void OnValueChanged(float value)
    {
        bar.value = value;
    }
}