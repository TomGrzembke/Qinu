using MyBox;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary> Attach this to the text component of a button to match its naming </summary>
public class UIButton : MonoBehaviour
{
    #region serialized fields
    [SerializeField] string buttonName;
    [SerializeField] bool additionalSettings;
    [SerializeField] Color hoverCol;
    [SerializeField, ConditionalField(nameof(additionalSettings))] float scaleOnClick = .75f;
    [SerializeField, ConditionalField(nameof(additionalSettings))] float scaleTime = .075f;
    #endregion

    #region private fields
    Color normalCol;
    readonly string buttonNameSyntax = "[Button]";
    readonly string textNameSyntax = "[Text]";
    TextMeshProUGUI textComponent;
    Coroutine scaleRoutine;
    #endregion

    void OnValidate()
    {
        name = buttonNameSyntax + " " + buttonName;
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent == null) return;
        textComponent.text = buttonName;
        textComponent.name = textNameSyntax + " " + buttonName;
    }
    void Awake()
    {
        normalCol = textComponent.color;
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
            textComponent.color = hoverCol;
        else
            textComponent.color = normalCol;

    }
}