using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour
{
    public event Action<DialogueBox> DialogueContinued;

    #region Inspector
    [SerializeField] float typeSpeedMultiplier = 1f;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] Button continueButton;
    [field: SerializeField] public FollowGameObjectInCam CamFollow { get; private set; } 
    #endregion

    #region private
    Coroutine displayLineCoroutine;
    float typeSpeed => DialogueController.Instance.TypeSpeed / typeSpeedMultiplier;
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

        displayLineCoroutine = StartCoroutine(DisplayLine(dialogueLine.text, dialogueLine.speaker));

    }

    IEnumerator DisplayLine(string line, string speaker)
    {
        dialogueText.text = "";

        for (int i = 0; i < line.Length; i++)
        {
            dialogueText.text += line[i];

            yield return new WaitForSeconds(typeSpeed);
        }
        yield return new WaitForSeconds(1f);

        DialogueController.Instance.ContinueDialogue();
        transform.parent.gameObject.SetActive(false);
    }
    #endregion
}
