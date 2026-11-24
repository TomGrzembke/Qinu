using System.Collections;
using UnityEngine;

public class TimeScaler : MonoBehaviour
{
    [ShowOnly, SerializeField] float desiredTimescale = 1;
    bool paused;

    public static TimeScaler Instance;
    Coroutine blendRoutine;

    void Awake()
    {
        Instance = this;
        ApplyScale();
    }

    public void SetPaused(bool condition)
    {
        paused = condition;

        if (paused)
        {
            desiredTimescale = 0;
        }
        else
        {
            desiredTimescale = 1;
        }

        ApplyScale();
    }

    void ApplyScale()
    {
        Time.timeScale = desiredTimescale;
    }

    public void PlaySlowMo(SlowMoDataSo slowMoDataSo)
    {
        if (blendRoutine != null)
        {
            StopCoroutine(blendRoutine);
        }

        blendRoutine = StartCoroutine(BlendTimeScaleRoutine(slowMoDataSo));
    }

    public bool GetSlowMoFinished()
    {
        return blendRoutine == null;
    }

    IEnumerator BlendTimeScaleRoutine(SlowMoDataSo slowMoDataSo)
    {
        var slowMoData = slowMoDataSo.SlowMoData;

        yield return BlendOneWay(1, slowMoData.InSlowMoBlendTimeScale, slowMoData.InSlowMoBlendTime, slowMoData.InSlowMoCurve);
        yield return BlendOneWay(slowMoData.InSlowMoBlendTimeScale, 1, slowMoData.OutSlowMoBlendTime, slowMoData.OutSlowMoCurve);

        blendRoutine = null;
    }

    IEnumerator BlendOneWay(float fromValue, float toValue, float totalTime, AnimationCurve animationCurve)
    {
        float timeBlended = 0;
        float currentStep;

        while (timeBlended < totalTime)
        {
            if (desiredTimescale <= 0f) //Respect Pause
            {
                yield return null;
                continue;
            }

            timeBlended += Time.unscaledDeltaTime;

            currentStep = timeBlended / totalTime;

            if (animationCurve != null)
            {
                currentStep = animationCurve.Evaluate(currentStep);
            }

            desiredTimescale = Mathf.Lerp(fromValue, toValue, currentStep);
            ApplyScale();
            yield return null;
        }
    }

}
