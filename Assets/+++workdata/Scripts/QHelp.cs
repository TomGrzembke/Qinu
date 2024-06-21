using System.Collections;
using UnityEngine;

public class QHelp : MonoBehaviour
{
    #region serialized fields
    [SerializeField] float waitTillHelp = 7;
    [SerializeField] string dialoguePath = "QHelp";
    #endregion

    #region private fields
    Coroutine qHelpCor;
    #endregion

    public void InitializeHelp()
    {
        qHelpCor = StartCoroutine(QHelpCor());
        StartCoroutine(QCheckCor());
    }

    IEnumerator QHelpCor()
    {
        yield return new WaitForSeconds(waitTillHelp);
        DialogueController.Instance.StartDialogue(dialoguePath);
    }

    IEnumerator QCheckCor()
    {
        yield return new WaitUntil(() => InputManager.Instance.Ability0Action.IsPressed());
        StopCoroutine(qHelpCor);
    }
}