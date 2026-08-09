using Ink;
using Ink.Runtime;
using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public struct DialogueLine
{
    public string speaker;

    public string text;

    public List<Choice> choices;
}

/// <summary> Handles all incoming dialogue calls</summary>
public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance;

    [field: SerializeField] public float TimeBetweenDialogue { get; private set; } = 1;
    [SerializeField] TextAsset inkAsset;
    [field: SerializeField] public bool InDialogue { get; private set; }
    [field: SerializeField] public float TypeSpeed { get; private set; } = 0.05f;
    [SerializeField] Transform offText;
    [SerializeField] GameObject[] speakerBoxParents;
    [SerializeField] Transform topLeft;
    [SerializeField] Transform topRight;
    [SerializeField] Transform botLeft;
    [SerializeField] Transform botRight;

    [Header("Debug")]
    [SerializeField] bool debugMessages;

    public static Action dialogueOpened;
    public static Action dialogueClosed;
    public static Action<string> InkEvent;
    const string SPEAKER_TAG = "speaker";
    List<string> queuedDialogue = new();
    float oldSpeed;
    string lastSpeaker;
    Story inkStory;
    Coroutine debugTextSpeedCor;
    Coroutine dialogueQueueCor;

    void Awake()
    {
        Instance = this;
        inkStory = new(inkAsset.text);
        inkStory.onError += OnInkError;
        inkStory.BindExternalFunction<string>("Event", Event);
        oldSpeed = TypeSpeed;
    }


    void OnDestroy()
    {
        inkStory.onError -= OnInkError;
    }

    public void StartDialogue(string dialoguePath)
    {
        if (InDialogue)
        {
            queuedDialogue.Add(dialoguePath);

            if (dialogueQueueCor != null)
            {
                StopCoroutine(dialogueQueueCor);
            }

            dialogueQueueCor = StartCoroutine(HandleDialogueQueue());

            if (debugMessages)
            {
                Debug.Log("Queued dialogue: " + dialoguePath);
            }

            return;
        }

        if (debugMessages)
        {
            Debug.Log("Starting dialogue: " + dialoguePath);
        }

        OpenDialogue();
        try
        {
            inkStory.ChoosePathString(dialoguePath);
        }
        catch
        {
            Debug.LogWarning(dialoguePath + " doesn't exist");
        }

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
        {
            if (lastSpeaker != null)
            {
                dialogueLine.speaker = lastSpeaker;
            }
        }

        lastSpeaker = dialogueLine.speaker;


        DialogueBox dialogueBox = GetDialogueBox(dialogueLine);

        if (!dialogueBox) return;

        SetDialoguePlacement(dialogueBox, dialogueLine);
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
        {
            Debug.Log(dialogueLine.speaker + " has no DialogueBox in " + nameof(speakerBoxParents));
        }

        return null;
    }

    void SetDialoguePlacement(DialogueBox dialogueBox, DialogueLine dialogueLine)
    {
        string speaker = dialogueLine.speaker;
        if (!CharManager.Instance)
        {
            LogMissingSpeaker(speaker);
            return;
        }

        List<GameObject> chars = CharManager.Instance.CharsSpawned;
        chars.CleanList();

        for (int i = 0; i < chars.Count; i++)
        {
            if (!chars[i].name.Contains(speaker)) continue;

            NPCNav nPCNav = chars[i].GetComponentInChildren<NPCNav>();
            bool isRightSide = nPCNav
                ? nPCNav.IsRight
                : IsOnRightSideOfCamera(chars[i].transform);

            dialogueBox.CamFollow?.SetDialoguePlacement(
                chars[i].transform,
                isRightSide,
                topLeft,
                topRight,
                botLeft,
                botRight);
            return;
        }

        LogMissingSpeaker(speaker);
    }

    bool IsOnRightSideOfCamera(Transform speaker)
    {
        Camera mainCamera = Camera.main;
        return mainCamera && mainCamera.WorldToViewportPoint(speaker.position).x >= .5f;
    }

    void LogMissingSpeaker(string speaker)
    {
        if (debugMessages)
            Debug.Log(speaker + " isn't in scene");
    }

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
                case SPEAKER_TAG:
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

    public void BoostTypeSpeed(float typeTime)
    {
        if (debugTextSpeedCor != null)
        {
            StopCoroutine(debugTextSpeedCor);
        }

        debugTextSpeedCor = StartCoroutine(BoostTypeSpeedCoroutine(typeTime));
    }

    IEnumerator BoostTypeSpeedCoroutine(float typeTime)
    {
        TypeSpeed /= 10;
        yield return new WaitForSeconds(typeTime);
        TypeSpeed = oldSpeed;
    }

    IEnumerator HandleDialogueQueue()
    {
        while (InDialogue)
        {
            yield return null;
        }

        var dialogueIndex = queuedDialogue.Count - 1;
        var dialoguePath = queuedDialogue[dialogueIndex];

        StartDialogue(dialoguePath);
        queuedDialogue.Remove(dialoguePath);

        if (queuedDialogue.Count == 0) yield break;

        dialogueQueueCor = StartCoroutine(HandleDialogueQueue());
    }
}
