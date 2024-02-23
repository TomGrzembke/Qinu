using TMPro;
using UnityEngine;

/// <summary> Attach this to the text component of a button to match its naming </summary>
public class UIButton : MonoBehaviour
{
    #region serialized fields
    [SerializeField] string buttonName;
    #endregion

    #region private fields
    readonly string buttonNameSyntax = "[Button]";
    readonly string textNameSyntax = "[Text]";
    TextMeshProUGUI textComponent;
    #endregion

    void OnValidate()
    {
        name = buttonNameSyntax + " " + buttonName;
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent == null) return;
        textComponent.text = buttonName;
        textComponent.name = textNameSyntax + " " + buttonName;
    }
}