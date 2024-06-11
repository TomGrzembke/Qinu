using System.Collections;
using UnityEngine;

public class IntroManager : MonoBehaviour
{
    #region serialized fields
    [SerializeField] GameObject Cage;
    [SerializeField] GameObject ballObject;
    [SerializeField] CanvasGroup scoreCG;
    [SerializeField] CanvasGroup uiCG;
    [SerializeField] NPCNav anthonyNav;
    [SerializeField] float waitBeforeSpeaking = 2;

    [SerializeField] string[] dialogues;
    #endregion

    #region private fields
    int dialogueID = -1;

    #endregion

    void Start()
    {
        StartCoroutine(IntroCoroutine());
    }

    IEnumerator IntroCoroutine()
    {
        yield return new WaitForSeconds(waitBeforeSpeaking);
        DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);
    }
}