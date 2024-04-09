using Ink;
using Ink.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public struct DialogueLine
{
    public string speaker;

    public string text;

    public List<Choice> choices;
}

public class DialogueController : MonoBehaviour
{
    const string speakerTag = "speaker";
    public static Action dialogueOpened;
    public static Action dialogueClosed;

    public static Action<string> InkEvent;
    GameState gameState;

    #region SerializeField

    [SerializeField] TextAsset inkAsset;

    [SerializeField] DialogueBox dialogueBox;
    #endregion

    #region private
    Story inkStory;
    #endregion

    #region other Syntax for Ink Speaker if speaker comes first or using a tag
    const string SpeakerSeparator = ":";
    const string EscapedColon = "::";
    const string EscapedColonPlaceholder = "§";

    DialogueLine ParseText(string inkLine, List<string> tags)
    {
        inkLine = inkLine.Replace(EscapedColon, EscapedColonPlaceholder);

        List<String> parts = inkLine.Split(SpeakerSeparator).ToList();
        string currentSpeaker;
        string currentText;

        switch (parts.Count)
        {
            case 1:
                currentSpeaker = null;
                currentText = parts[0];
                break;
            case 2:
                currentSpeaker = parts[0];
                currentText = parts[1];
                break;
            default:
                Debug.LogWarning($"Ink dialogue line was split at more {SpeakerSeparator} than extpected. " +
                                 $"Please make sure to use {EscapedColon} for {SpeakerSeparator} inside text.", gameObject);
                goto case 2;
        }

        // For tags
        for (int i = 0; i < tags.Count; i++)
        {
            List<string> tagParts = tags[i].Split(SpeakerSeparator).ToList();

            switch (tagParts[0])
            {
                case speakerTag:
                    currentSpeaker = tagParts[1];
                    //logic for portraits filtered after names
                    break;
                default:
                    break;
            }
        }

        DialogueLine line = new()
        {
            speaker = currentSpeaker?.Trim(),
            text = currentText?.Trim().Replace(EscapedColonPlaceholder, SpeakerSeparator)
        };
        return line;
    }
    #endregion

    #region UnityEvent Functions
    void Awake()
    {
        gameState = FindObjectOfType<GameState>();
        inkStory = new (inkAsset.text);
        inkStory.onError += OnInkError;
        inkStory.BindExternalFunction<string>("Event", Event);
        inkStory.BindExternalFunction<string>("GetState", Get_State);
        inkStory.BindExternalFunction<string, int>("AddState", Add_State);

    }
     void Start()
    {
        dialogueBox.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        dialogueBox.DialogueContinued += OnDialogueContinued;
        dialogueBox.ChoiceSelected += OnChoiceSelected;
    }

    void OnDisable()
    {
        dialogueBox.DialogueContinued -= OnDialogueContinued;
        dialogueBox.ChoiceSelected -= OnChoiceSelected;
    }

    #endregion

    void OnDestroy()
    {
        inkStory.onError -= OnInkError;
    }

    #region DialogueLifeCycle
    public void StartDialogue(string dialoguePath)
    {
        OpenDialogue();
        inkStory.ChoosePathString(dialoguePath);
        ContinueDialogue();
    }

    void ContinueDialogue()
    {
        if (IsAtEnd())
        {
            CloseDialogue();
            return;
        }

        DialogueLine dialogueLine = new();

        if (CanContinue())
        {
            string inkLine = inkStory.Continue();

            if (string.IsNullOrWhiteSpace(inkLine))
            {
                ContinueDialogue();
                return;
            }

            dialogueLine.text = inkLine;
        }

        dialogueLine.choices = inkStory.currentChoices;

        if (inkStory.currentTags?[0] != null)
        {
            dialogueLine = HandleTags(inkStory.currentTags, dialogueLine);
        }

        dialogueBox.DisplayText(dialogueLine);
    }

    void OpenDialogue()
    {
        dialogueBox.gameObject.SetActive(true);
        dialogueOpened?.Invoke();
    }

    void CloseDialogue()
    {
        EventSystem.current.SetSelectedGameObject(null);
        dialogueBox.gameObject.SetActive(false);
        dialogueClosed?.Invoke();
    }

    void SelectChoice(int choiceIndex)
    {
        inkStory.ChooseChoiceIndex(choiceIndex);
        ContinueDialogue();
    }
    #endregion

    #region Ink
    DialogueLine HandleTags(List<string> currentTags, DialogueLine dialogueLine)
    {
        foreach (string tag in currentTags)
        {
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2)
            {
                Debug.LogError(tag + " hasnt been split correctly");
                break;
            }
            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();

            switch (tagKey)
            {
                case speakerTag:
                    dialogueLine.speaker = tagValue;
                    break;
                default:
                    Debug.LogError("There is no " + tag + " in this switchcase", gameObject);
                    break;
            }
        }
        return dialogueLine;
    }

    bool CanContinue()
    {
        return inkStory.canContinue;
    }
    bool HasChoice()
    {
        return inkStory.currentChoices.Count > 0;
    }
    bool IsAtEnd()
    {
        return !CanContinue() && !HasChoice();
    }

    void OnInkError(string message, ErrorType type)
    {
        switch (type)
        {
            case ErrorType.Author:
                Debug.LogError(message);
                break;
            case ErrorType.Warning:
                Debug.LogWarning(message);
                break;
            case ErrorType.Error:
                Debug.LogError(message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    void OnChoiceSelected(DialogueBox dialogueBox, int choiceIndex)
    {
        SelectChoice(choiceIndex);
    }
    void OnDialogueContinued(DialogueBox dialogueBox)
    {
        ContinueDialogue();
    }
    void Event(string eventName)
    {
        InkEvent?.Invoke(eventName);
    }

    object Get_State(string id)
    {
        State state = gameState.Get(id);
        return state != null ? state.amount : null;
    }

    void Add_State(string id, int amount)
    {
        gameState.Add(id, amount);
    }
    #endregion
}