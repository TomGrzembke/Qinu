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

        displayLineCoroutine = StartCoroutine(DisplayLine(dialogueLine));

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
    #endregion
}
