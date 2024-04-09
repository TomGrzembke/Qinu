using Ink;
using Ink.Runtime;
using System;
using System.Collections.Generic;
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
    public static Action dialogueOpened;
    public static Action dialogueClosed;
    public static Action<string> InkEvent;

    #region SerializeField
    [SerializeField] TextAsset inkAsset;
    [SerializeField] DialogueBox dialogueBox;
    #endregion

    #region private
    const string speakerTag = "speaker";
    GameState gameState;
    Story inkStory;
    #endregion

    #region UnityEvent Functions
    void Awake()
    {
        gameState = FindObjectOfType<GameState>();
        inkStory = new(inkAsset.text);
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
    }

    void OnDisable()
    {
        dialogueBox.DialogueContinued -= OnDialogueContinued;
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

    bool IsAtEnd()
    {
        return !CanContinue();
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
        return state?.amount;
    }

    void Add_State(string id, int amount)
    {
        gameState.Add(id, amount);
    }
    #endregion
}