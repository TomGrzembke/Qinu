using UnityEngine;

public class InkDialogue : MonoBehaviour
{
    #region Inspector
    [SerializeField] string dialoguePath;
    #endregion

    public void StartDialogue()
    {
        if (string.IsNullOrWhiteSpace(dialoguePath))
        {
            Debug.LogError("No dialogue path given", this);
            return;
        }
    }
}
