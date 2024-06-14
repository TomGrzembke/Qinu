using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour
{
    public event Action<DialogueBox> DialogueContinued;

    #region Inspector
    [SerializeField] SoundType voice;
    [SerializeField] float typeSpeedMultiplier = 1f;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] Button continueButton;
    [field: SerializeField] public FollowGameObjectInCam CamFollow { get; private set; }

    #endregion

    #region private
    Coroutine displayLineCoroutine;
    float typeSpeed => DialogueController.Instance.TypeSpeed / typeSpeedMultiplier;
    const string textAlpha = "<color=#00000000>";
    #endregion

    #region UnityEvents
    void Awake()
    {
        continueButton?.onClick.AddListener(() => { DialogueContinued?.Invoke(this); });
    }

    void OnEnable()
    {
        dialogueText.SetText(string.Empty);
    }

    public void DisplayText(DialogueLine dialogueLine)
    {
        if (displayLineCoroutine != null)
            StopCoroutine(displayLineCoroutine);

        displayLineCoroutine = StartCoroutine(DisplayLineAlpha(dialogueLine));

    }

    IEnumerator DisplayLine(DialogueLine dialogueLine)
    {
        dialogueText.text = "";

        for (int i = 0; i < dialogueLine.text.Length; i++)
        {
            dialogueText.text += dialogueLine.text[i];

            yield return new WaitForSeconds(typeSpeed);
        }
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

        foreach (char c in originalText.ToCharArray())
        {
            alphaIndex++;
            dialogueText.text = originalText;

            displayedText = dialogueText.text.Insert(alphaIndex, textAlpha);
            dialogueText.text = displayedText;
            SoundManager.Instance.PlayVoice(voice);

            yield return new WaitForSeconds(typeSpeed);

        }

        yield return new WaitForSeconds(1f);

        transform.parent.gameObject.SetActive(false);
        DialogueController.Instance.ContinueDialogue();
    }
    #endregion
}
