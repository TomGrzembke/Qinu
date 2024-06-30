using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeCanvasGroup : MonoBehaviour
{
    #region Serialized
    [field: SerializeField] public float FadeTime { get; private set; } = 1;
    #endregion

    #region Non Serialized
    public static FadeCanvasGroup Instance;
    Dictionary<CanvasGroup, Coroutine> fadeCoroutines = new();
    #endregion

    void Awake()
    {
        Instance = this;
    }

    void OnValidate()
    {
        if (FadeTime == 0)
            FadeTime = .1f;
    }

    public void FadeIn(CanvasGroup canvasGroup)
    {
        DictionarySorting(canvasGroup, true);
    }

    public void FadeOut(CanvasGroup canvasGroup)
    {
        DictionarySorting(canvasGroup, false);
    }

    void DictionarySorting(CanvasGroup canvasGroup, bool fadeIn)
    {
        if (fadeCoroutines.ContainsKey(canvasGroup))
            StopCoroutine(fadeCoroutines[canvasGroup]);

        Coroutine coroutine = StartCoroutine(Fade(canvasGroup, fadeIn));

        if (!fadeCoroutines.ContainsKey(canvasGroup))
            fadeCoroutines.Add(canvasGroup, coroutine);
    }

    IEnumerator Fade(CanvasGroup canvasGroup, bool fadeIn)
    {
        float timeFaded = 0;
        float currentAlpha = canvasGroup.alpha;

        while (timeFaded < FadeTime)
        {
            canvasGroup.alpha = Mathf.Lerp(currentAlpha, fadeIn ? 1 : 0, timeFaded / FadeTime);
            timeFaded += Time.deltaTime;
            yield return null;
        }
    }
}