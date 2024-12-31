using System.Collections;
using UnityEngine;

public class QHelp : MonoBehaviour
{
    #region Serialized
    [SerializeField] float waitTillHelp = 7;
    [SerializeField] string dialoguePath = "QHelp";
    #endregion

    #region Non Serialized
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
        yield return new WaitUntil(() => InputManager.Instance.Ability0Action.IsPressed() || AbilitySlotManager.Instance.GetAbilitySlotPerformed(0));
        StopCoroutine(qHelpCor);
    }
}