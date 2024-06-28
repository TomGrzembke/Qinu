using MyBox;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary> Attach this to the text component of a button to match its naming and get some basic scale animations </summary>
public class UIButton : MonoBehaviour
{
    #region Serialized
    [SerializeField] string buttonName;
    [SerializeField] bool additionalSettings;
    [SerializeField, ConditionalField(nameof(additionalSettings))] float scaleOnClick = .75f;
    [SerializeField, ConditionalField(nameof(additionalSettings))] float scaleTime = .075f;
    [SerializeField, ConditionalField(nameof(additionalSettings))] float scaleHover = .95f;
    #endregion

    #region Non Serialized
    readonly string buttonNameSyntax = "[Button]";
    readonly string textNameSyntax = "[Text]";
    TextMeshProUGUI textComponent;
    Coroutine scaleRoutine;
    #endregion

    void OnValidate() => OnValidateCall();

    void Awake() => OnValidateCall();

    private void OnValidateCall()
    {
        name = buttonNameSyntax + " " + buttonName;
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent == null) return;
        textComponent.text = buttonName;
        textComponent.name = textNameSyntax + " " + buttonName;
    }

    public void ClickedAnim()
    {
        transform.localScale = Vector3.one;

        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(ScaleAnim());
    }

    IEnumerator ScaleAnim()
    {
        float scaledTime = 0;
        while (scaledTime < scaleTime)
        {
            scaledTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(scaleOnClick, scaleOnClick), scaledTime / scaleTime);
            yield return null;
        }

        scaledTime = 0;
        while (scaledTime < scaleTime)
        {
            scaledTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, scaledTime / scaleTime);
            yield return null;
        }
    }

    public void OnHover(bool condition)
    {
        if (condition)
            transform.localScale = new Vector3(scaleHover, scaleHover);
        else
            transform.localScale = Vector3.one;
    }
}