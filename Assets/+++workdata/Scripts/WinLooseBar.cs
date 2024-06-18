using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WinLooseBar : MonoBehaviour
{
    #region serialized fields
    [SerializeField] Slider bar;
    [SerializeField] float changeTime = 2;
    [SerializeField] AnimationCurve animCurve;
    #endregion
    Coroutine cor;

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
        cor = StartCoroutine(ChangeValue(value));
    }

    IEnumerator ChangeValue(float value)
    {
        float timeFaded = 0;

        while (timeFaded < changeTime)
        {
            bar.value = Mathf.Lerp(bar.value, value, changeTime / changeTime);
            timeFaded += Time.deltaTime;
            yield return null;
        }
    }
}