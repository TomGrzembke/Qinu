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
    public static DialogueController Instance;
    public static Action dialogueOpened;
    public static Action dialogueClosed;
    public static Action<string> InkEvent;

    #region SerializeField
    [SerializeField] TextAsset inkAsset;
    [field: SerializeField] public bool InDialogue { get; private set; }
    [field: SerializeField] public float TypeSpeed { get; private set; } = 0.05f;
    [SerializeField] Transform offText;
    [SerializeField] GameObject[] speakerBoxParents;
    [SerializeField] bool debugMessages;
    #endregion

    #region private
    const string speakerTag = "speaker";
    string lastSpeaker;
    Story inkStory;
    #endregion

    #region UnityEvent Functions
    void Awake()
    {
        Instance = this;
        inkStory = new(inkAsset.text);
        inkStory.onError += OnInkError;
        inkStory.BindExternalFunction<string>("Event", Event);
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

    public void ContinueDialogue()
    {
        if (!CanContinue())
        {
            CloseDialogue();
            return;
        }

        DialogueLine dialogueLine = new();

        string inkLine = inkStory.Continue();

        if (string.IsNullOrWhiteSpace(inkLine))
        {
            ContinueDialogue();
            return;
        }

        dialogueLine.text = inkLine;


        if (inkStory.currentTags.Count > 0)
        {
            dialogueLine = HandleTags(inkStory.currentTags, dialogueLine);
        }


        if (dialogueLine.speaker == null)
            if (lastSpeaker != null)
                dialogueLine.speaker = lastSpeaker;

        lastSpeaker = dialogueLine.speaker;


        DialogueBox dialogueBox = GetDialogueBox(dialogueLine);

        if (!dialogueBox) return;

        dialogueBox.CamFollow?.SetTargets(GetDialogueTarget(dialogueLine, 0), GetDialogueTarget(dialogueLine, 0));
        dialogueBox.DisplayText(dialogueLine);

    }

    void OpenDialogue()
    {
        InDialogue = true;
        dialogueOpened?.Invoke();
    }

    void CloseDialogue()
    {
        EventSystem.current.SetSelectedGameObject(null);
        InDialogue = false;
        dialogueClosed?.Invoke();
    }

    DialogueBox GetDialogueBox(DialogueLine dialogueLine)
    {
        string speaker = dialogueLine.speaker;

        for (int i = 0; i < speakerBoxParents.Length; i++)
        {
            if (!speakerBoxParents[i].name.Contains(speaker)) continue;

            speakerBoxParents[i].gameObject.SetActive(true);
            return speakerBoxParents[i].GetComponentInChildren<DialogueBox>();
        }

        if (debugMessages)
            Debug.Log(dialogueLine.speaker + " has no DialogueBox in " + nameof(speakerBoxParents));
        return null;
    }

    Transform GetDialogueTarget(DialogueLine dialogueLine, int id = 0)
    {
        string speaker = dialogueLine.speaker;
        List<GameObject> chars = CharManager.Instance.CharsSpawned;

        for (int i = 0; i < chars.Count; i++)
        {
            if (!chars[i].name.Contains(speaker)) continue;

            if (id == 0)
                return chars[i].GetComponentInChildren<NPCNav>()?.TopTextTarget;
            else if (id == 1)
                return chars[i].GetComponentInChildren<NPCNav>()?.BotTextTarget;
        }

        if (debugMessages)
            Debug.Log(dialogueLine.speaker + " isn't in scene");
        return null;
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
    #endregion
}