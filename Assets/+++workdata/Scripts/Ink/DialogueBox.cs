using System.Collections;
using MyBox;
using TMPro;
using UnityEngine;

/// <summary> Prints the dialogue lines on call</summary>
public class DialogueBox : MonoBehaviour
{
    [SerializeField] SoundType voice;
    [SerializeField] float typeSpeedMultiplier = 1f;
    [SerializeField] TextMeshProUGUI dialogueText;

    [SerializeField] GameObject continueObject;
    [SerializeField] Vector2 localContinueObjectOffset;
    [field: SerializeField] public FollowGameObjectInCam CamFollow { get; private set; }


    Coroutine displayLineCoroutine;
    float typeSpeed => DialogueController.Instance.TypeSpeed / typeSpeedMultiplier;
    const string textAlpha = "<color=#00000000>";

    bool continueTrigger;
    [field: SerializeField] public bool IsTyping { get; private set; } 


    void OnEnable()
    {
        dialogueText.SetText(string.Empty);
        continueObject.SetActive(false);
    }

    public void SetContinueTrigger(bool condition)
    {
        continueTrigger = condition;
    }

    public void DisplayText(DialogueLine dialogueLine)
    {
        if (displayLineCoroutine != null)
        {
            StopCoroutine(displayLineCoroutine);
        }

        displayLineCoroutine = StartCoroutine(DisplayLineAlpha(dialogueLine));

    }

    IEnumerator DisplayLine(DialogueLine dialogueLine)
    {
        dialogueText.text = "";
        IsTyping = true;
        for (int i = 0; i < dialogueLine.text.Length; i++)
        {
            dialogueText.text += dialogueLine.text[i];

            yield return new WaitForSeconds(typeSpeed);
        }
        IsTyping = false;
        yield return new WaitForSeconds(1f);

        transform.parent.gameObject.SetActive(false);
        DialogueController.Instance.ContinueDialogue();
    }

    IEnumerator DisplayLineAlpha(DialogueLine dialogueLine)
    {
        dialogueText.text = "";
        string originalText = dialogueLine.text;
        string displayedText = "";
        int alphaIndex = 0;
        IsTyping = true;

        foreach (char c in originalText.ToCharArray())
        {
            alphaIndex++;
            dialogueText.text = originalText;

            displayedText = dialogueText.text.Insert(alphaIndex, textAlpha);
            dialogueText.text = displayedText;
            SoundManager.Instance.PlayVoice(voice);

            yield return new WaitForSeconds(typeSpeed);

        }
        
        IsTyping = false;
        RefreshContinueIcon();

        //yield return new WaitForSeconds(DialogueController.Instance.TimeBetweenDialogue);
        yield return new WaitUntil(() => continueTrigger);
        continueTrigger = false;

        continueObject.SetActive(false);
        transform.parent.gameObject.SetActive(false);
        DialogueController.Instance.ContinueDialogue();
    }

    void RefreshContinueIcon()
    {
        TMP_TextInfo textInfo = dialogueText.textInfo;
        int lastIndex = GetLastVisibleCharacterIndex(textInfo);

        TMP_CharacterInfo charInfo = textInfo.characterInfo[lastIndex];
        Vector3 targetLocalPos = charInfo.bottomRight + localContinueObjectOffset.WithZ(charInfo.bottomRight.z);

        Vector3 worldPos = dialogueText.transform.TransformPoint(targetLocalPos);
        continueObject.transform.position = worldPos;

        continueObject.SetActive(true);
    }

    private int GetLastVisibleCharacterIndex(TMP_TextInfo textInfo)
    {
        int count = textInfo.characterCount;
        for (int i = count - 1; i >= 0; i--)
        {
            if (textInfo.characterInfo[i].isVisible) return i;
        }

        return -1;
    }
}
