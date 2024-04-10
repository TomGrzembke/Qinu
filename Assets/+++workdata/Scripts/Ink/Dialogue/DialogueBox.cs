using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Ink.Runtime;
using System.Collections.Generic;
using System.Collections;

public class DialogueBox : MonoBehaviour
{
    public event Action<DialogueBox> DialogueContinued;
    public event Action<DialogueBox, int> ChoiceSelected;

    #region Inspector
    [SerializeField] float typeSpeed = 0.03f;

    [SerializeField] TextMeshProUGUI dialogueSpeaker;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] Button continueButton;
    #endregion

    #region private
    Coroutine displayLineCoroutine;
    #endregion

    #region UnityEvents
    void Awake()
    {
        continueButton?.onClick.AddListener(() => { DialogueContinued?.Invoke(this); });
    }

    void OnEnable()
    {
        dialogueSpeaker?.SetText(string.Empty);
        dialogueText.SetText(string.Empty);
    }

    public void DisplayText(DialogueLine dialogueLine)
    {
        if (dialogueLine.speaker != null)
        {
            dialogueSpeaker?.SetText(dialogueLine.speaker);
        }

        if (displayLineCoroutine != null)
            StopCoroutine(displayLineCoroutine);

        displayLineCoroutine = StartCoroutine(DisplayLine(dialogueLine.text, dialogueLine.speaker));

        DisplayButtons(dialogueLine.choices);
    }

    IEnumerator DisplayLine(string line, string speaker)
    {
        dialogueText.text = "";

        for (int i = 0; i < line.Length; i++)
        {
            dialogueText.text += line[i];

            yield return new WaitForSeconds(typeSpeed);
        }

    }

    void DisplayButtons(List<Choice> choices)
    {
        Selectable newSelection;

        ShowContinueButton(true);
        newSelection = continueButton;
        StartCoroutine(DelayedSelection(newSelection));
    }

    public void ShowContinueButton(bool show)
    {
        continueButton?.gameObject.SetActive(show);
    }
    #endregion

    IEnumerator DelayedSelection(Selectable selectable)
    {
        yield return null;
        selectable?.Select();
    }
}
