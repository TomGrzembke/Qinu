using MyBox;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WinLooseBar : MonoBehaviour
{
    #region Serialized
    [SerializeField] Slider bar;
    [SerializeField] float changeTime = 2;
    [SerializeField] float fadeOut = 6;
    [SerializeField] AnimationCurve animCurve;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] ParticleSystem wonSystem;
    #endregion

    #region Non Serialized
    Coroutine cor;
    #endregion


    IEnumerator Start()
    {
        yield return null;
        TournamentManager.Instance.RegisterOnPlayerMatchEnd(OnValueChanged);
    }

    void OnDisable() => TournamentManager.Instance.OnPlayerMatchEnd -= OnValueChanged;

    void OnValueChanged(float value)
    {
        if (cor != null)
            StopCoroutine(cor);

        cor = StartCoroutine(ChangeValue(value));
    }

    IEnumerator ChangeValue(float value)
    {
        if (canvasGroup.alpha == 0)
        {
            FadeCanvasGroup.Instance.FadeIn(canvasGroup);
            yield return new WaitForSeconds(FadeCanvasGroup.Instance.FadeTime);
        }

        canvasGroup.alpha = 1;
        float timeFaded = 0;
        float barStartValue = bar.value;

        while (timeFaded < changeTime)
        {
            timeFaded += Time.deltaTime;
            bar.value = Mathf.Lerp(barStartValue, value, animCurve.Evaluate(timeFaded / changeTime));
            yield return null;
        }
        bar.value = value;

        if (value > 0)
            wonSystem.Play();

        yield return new WaitForSeconds(fadeOut);
        FadeCanvasGroup.Instance.FadeOut(canvasGroup);
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