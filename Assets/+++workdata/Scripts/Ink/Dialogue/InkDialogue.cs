using UnityEngine;

public class InkDialogue : MonoBehaviour
{
    #region Inspector
    [SerializeField] string dialoguePath;
    [SerializeField] DialogueController dialogueController;
    #endregion

    public void StartDialogue()
    {
        if (string.IsNullOrWhiteSpace(dialoguePath))
        {
            Debug.LogError("No dialogue path given", this);
            return;
        }

        dialogueController.StartDialogue(dialoguePath);
    }
}
