using MyBox;
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
        if (cor != null)
            StopCoroutine(cor);

        cor = StartCoroutine(ChangeValue(value));
    }

    IEnumerator ChangeValue(float value)
    {
        float timeFaded = 0;
        float barStartValue = bar.value;

        while (timeFaded < changeTime)
        {
            timeFaded += Time.deltaTime;
            bar.value = Mathf.Lerp(barStartValue, value, animCurve.Evaluate(timeFaded / changeTime));
            yield return null;
        }
        bar.value = value;
    }

    [ButtonMethod]
    public void TestPlusOne()
    {
        OnValueChanged(bar.value + 1);
    }
    [ButtonMethod]
    public void TestMinusOne()
    {
        OnValueChanged(bar.value - 1);
    }
}