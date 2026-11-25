using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkipTutorial : MonoBehaviour
{
    DialogueTutorial[] dialogueTutorials;
    [SerializeField] Button button;

    void OnEnable()
    {
        dialogueTutorials = FindObjectsByType<DialogueTutorial>();
        StartCoroutine(QueueCheckButton());
    }

    IEnumerator QueueCheckButton()
    {
        yield return null;
        CheckButtonActive();
    }

    void CheckButtonActive()
    {
        bool isInTutorial = false;

        foreach (var tutorial in dialogueTutorials)
        {
            if (tutorial == null) continue;
            if (!tutorial.IsInTutorial()) continue;

            isInTutorial = true;
            break;
        }

        button.gameObject.SetActive(isInTutorial);
    }

    public void FinishTutorial()
    {
        foreach (var tutorial in dialogueTutorials)
        {
            if (tutorial == null) continue;

            tutorial.SkipTutorial();
        }

        InputManager.Instance.ShowCursor();
    }

}
