using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LoadingScreen : MonoBehaviour
{
    [SerializeField] float fadeTime = 2;
    [SerializeField] float loadingTime = 1;
    [SerializeField] CanvasGroup canvasGroup;

    static List<object> loadingInstigator = new();
    public static LoadingScreen Instance;

    void OnDestroy()
    {
        loadingInstigator.Clear();
        StopAllCoroutines();
    }

    void Awake()
    {
        Instance = this;

        OnValidateCall();
        Initialize();
    }

    void OnValidate() => OnValidateCall();

    void OnValidateCall()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    public void Show(object instigator)
    {
        loadingInstigator.Add(instigator);
        Show();
    }

    public void Hide(object instigator)
    {
        loadingInstigator.Remove(instigator);

        if (loadingInstigator.Count == 0)
        {
            Hide();
        }
    }

    public void Initialize()
    {
        loadingInstigator.Clear();

        if (loadingInstigator.Count > 0)
        {
            Show();
        }
    }

    public void Show()
    {
        if (canvasGroup == null) return;

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
}