using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Ink.Runtime;
using System.Collections;

public class DialogueBox : MonoBehaviour
{
    public event Action<DialogueBox> DialogueContinued;
    public event Action<DialogueBox, int> ChoiceSelected;

    #region Inspector
    [Header("General")]
    [SerializeField] float typeSpeed = 0.03f;
    [Header("Sound")]
    //[SerializeField] DialogueSoundPlayer dialogueSoundPlayer;
    [SerializeField] Transform currentSoundOrigin;

    [SerializeField] TextMeshProUGUI dialogueSpeaker;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] Button continueButton;

    [Header("Choices")]
    [SerializeField] Transform choiceContainer;
    [SerializeField] Button choiceButtonPrefab;
    #endregion

    #region private
    Coroutine displayLineCoroutine;
    #endregion

    #region UnityEvents
    void Awake()
    {
        continueButton.onClick.AddListener(() =>{ DialogueContinued?.Invoke(this); });
    }

    void OnEnable()
    {
        dialogueSpeaker.SetText(string.Empty);
        dialogueText.SetText(string.Empty);
    }

    public void DisplayText(DialogueLine dialogueLine)
    {
        if (dialogueLine.speaker != null)
        {
            dialogueSpeaker.SetText(dialogueLine.speaker);
        }

        if(displayLineCoroutine != null)
            StopCoroutine(displayLineCoroutine);

        displayLineCoroutine = StartCoroutine(DisplayLine(dialogueLine.text, dialogueLine.speaker));

        DisplayButtons(dialogueLine.choices);
    }

    IEnumerator DisplayLine(String line, string speaker)
    {
        dialogueText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;

            //if(currentSoundOrigin != null)
            //dialogueSoundPlayer.OnDialogue(speaker, currentSoundOrigin);

            yield return new WaitForSeconds(typeSpeed);
        }
    }

    void DisplayButtons(List<Choice> choices)
    {
        Selectable newSelection;

        if (choices == null || choices.Count == 0)
        {
            ShowContinueButton(true);
            Showchoices(false);
            newSelection = continueButton;
        }
        else
        {
            ClearChoices();
            List<Button> choiceButtons = GenerateChoices(choices);

            ShowContinueButton(false);
            Showchoices(true);
            newSelection = choiceButtons[0];
        }
        StartCoroutine(DelayedSelection(newSelection));
    }
    void ClearChoices()
    {
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void ShowContinueButton(bool show)
    {
        continueButton.gameObject.SetActive(show);
    }

    public void Showchoices(bool show)
    {
        choiceContainer.gameObject.SetActive(show);
    }

    List<Button> GenerateChoices(List<Choice> choices)
    {
        List<Button> choiceButtons = new List<Button>(choices.Count);

        for (int i = 0; i < choices.Count; i++)
        {
            Choice choice = choices[i];
            Button button = Instantiate(choiceButtonPrefab, choiceContainer);

            int index = i;
            button.onClick.AddListener(() => ChoiceSelected?.Invoke(this, index));


            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.SetText(choice.text);
            button.name = choice.text;

            choiceButtons.Add(button);
        }

        return choiceButtons;
    }
    #endregion

    IEnumerator DelayedSelection(Selectable selectable)
    {
        yield return null;
        selectable.Select();
    }

    public void SetSoundOrigin(Transform newOrigin)
    {
        currentSoundOrigin = newOrigin;
    }
}
