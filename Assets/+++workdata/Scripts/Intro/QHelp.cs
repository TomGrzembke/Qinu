using System.Collections;
using UnityEngine;

public class QHelp : MonoBehaviour
{
    [SerializeField] float waitTillHelp = 7;
    [SerializeField] string dialoguePath = "QHelp";

    Coroutine dashHelpCor;

    public void InitializeHelp()
    {
        dashHelpCor = StartCoroutine(DashHelpCor());
        StartCoroutine(DashCheckCor());
    }

    IEnumerator DashHelpCor()
    {
        yield return new WaitForSeconds(waitTillHelp);
        DialogueController.Instance.StartDialogue(dialoguePath);
    }

    IEnumerator DashCheckCor()
    {
        yield return new WaitUntil(() => InputManager.Instance.Ability0Action.IsPressed());

        if (dashHelpCor != null)
        {
            StopCoroutine(dashHelpCor);
        }
    }

    void OnDisable()
    {
        if (dashHelpCor != null)
        {
            StopCoroutine(dashHelpCor);
        }
    }
}