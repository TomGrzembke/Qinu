using UnityEngine;

public class InkDialogue : MonoBehaviour
{
    #region Inspector
    [SerializeField] string dialoguePath;
    [SerializeField] bool onStart;
    #endregion
    void Start()
    {
        if (onStart)
            StartDialogue();
    }

    public void StartDialogue()
    {
        if (string.IsNullOrWhiteSpace(dialoguePath))
        {
            Debug.LogError("No dialogue path given", this);
            return;
        }

        DialogueController.Instance.StartDialogue(dialoguePath);
    }
}
