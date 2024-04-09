using Ink;
using Ink.Runtime;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestBox : MonoBehaviour
{
    #region Inspector
    [Header("General")]

    [SerializeField] TextMeshProUGUI questText;
    [SerializeField] GameObject questbox;
    [SerializeField] Image checkMarkImage;
    [SerializeField] Sprite[] checkMarkStates;

    [Header("Ink")]
    [SerializeField] TextAsset inkAsset;
    public static Action<string> InkEvent;
    #endregion

    #region private
    Story inkStory;
    #endregion

    #region Ink
    private void Awake()
    {
        inkStory = new Story(inkAsset.text);
        inkStory.onError += OnInkError;
        inkStory.BindExternalFunction<string>("Event", Event);
    }
    void OnEnable()
    {
        questText.SetText(string.Empty);
    }
    public void StartQuest(string questContent = null)
    {
        questText.gameObject.SetActive(true);
        questbox.SetActive(true);
        inkStory.ChoosePathString(questContent);
        DisplayText();
        checkMarkImage.sprite = checkMarkStates[0];
    }
    public void DisplayText()
    {
        string inkLine = inkStory.Continue();
        questText.text = inkLine;
    }
    public void QuestDone()
    {
        checkMarkImage.sprite = checkMarkStates[1];
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
    void OnDestroy()
    {
        inkStory.onError -= OnInkError;
    }
    void Event(string eventName)
    {
        InkEvent?.Invoke(eventName);
    }
    #endregion
}
