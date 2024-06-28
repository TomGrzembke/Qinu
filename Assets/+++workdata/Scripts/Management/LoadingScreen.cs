using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LoadingScreen : MonoBehaviour
{
    #region Serialized
    [SerializeField] float fadeTime = 2;
    [SerializeField] float loadingTime = 1;
    [SerializeField] CanvasGroup canvasGroup;
    #endregion

    #region Non Serialized
    static List<object> loadingInstigator = new();
    #endregion

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
    }

    public void Show()
    {
        canvasGroup.alpha = 1;
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(HideCoroutine());
    }

    IEnumerator HideCoroutine()
    {
        yield return new WaitForSeconds(loadingTime);
        float time = 0;
        while (time < fadeTime)
        {
            yield return null;
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1 - Mathf.Clamp01(time / fadeTime);
        }
        canvasGroup.alpha = 0;
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