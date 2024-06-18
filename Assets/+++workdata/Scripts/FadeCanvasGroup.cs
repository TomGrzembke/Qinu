using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeCanvasGroup : MonoBehaviour
{
    [SerializeField] float fadeTime = 2;
    readonly Dictionary<CanvasGroup, Coroutine> fadeCoroutines = new();
    public static FadeCanvasGroup Instance;

    void Awake()
    {
        Instance = this;
    }

    public void FadeIn(CanvasGroup canvasGroup)
    {
        DictionarySorting(canvasGroup, true);
    }

    public void FadeOut(CanvasGroup canvasGroup)
    {
        DictionarySorting(canvasGroup, false);
    }

    private void DictionarySorting(CanvasGroup canvasGroup, bool fadeIn)
    {
        if (fadeCoroutines.ContainsKey(canvasGroup))
            StopCoroutine(fadeCoroutines[canvasGroup]);

        Coroutine coroutine = StartCoroutine(Fade(canvasGroup, fadeIn));
        fadeCoroutines.Add(canvasGroup, coroutine);
    }

    IEnumerator Fade(CanvasGroup canvasGroup, bool fadeIn)
    {
        float timeFaded = 0;

        while (timeFaded < fadeTime)
        {
            canvasGroup.alpha = Mathf.Lerp(fadeIn ? 0 : 1, fadeIn ? 1 : 0, timeFaded / fadeTime);
            timeFaded += Time.deltaTime;
            yield return null;
        }
    }
}