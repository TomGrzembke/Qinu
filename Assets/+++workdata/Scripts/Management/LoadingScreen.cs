using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LoadingScreen : MonoBehaviour
{
    [SerializeField] float fadeTime = 2;
    [SerializeField] CanvasGroup canvasGroup;

    static List<object> loadingInstigator = new();
    public static void Show(object instigator)
    {
        loadingInstigator.Add(instigator);
        if (ScreenManager.Instance != null)
            ScreenManager.Instance.LoadingScreen.Show();
    }
    public static void Hide(object instigator)
    {
        loadingInstigator.Remove(instigator);

        if (ScreenManager.Instance == null)
            return;

        if (loadingInstigator.Count == 0)
            ScreenManager.Instance.LoadingScreen.Hide();

    }

    public void Initialize()
    {
        if (loadingInstigator.Count > 0)
            Show();
        else
            gameObject.SetActive(false);
    }

    public void Show()
    {
        canvasGroup.alpha = 1;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(HideCoroutine());
    }

    IEnumerator HideCoroutine()
    {
        float time = 0;
        while (time < fadeTime)
        {
            yield return null;
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1 - Mathf.Clamp01(time / fadeTime);
        }
        canvasGroup.alpha = 0;
        gameObject.SetActive(false);

    }

    void Awake() => OnValidateCall();

    void OnValidate()
    {
        OnValidateCall();
    }

    void OnValidateCall()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }
}