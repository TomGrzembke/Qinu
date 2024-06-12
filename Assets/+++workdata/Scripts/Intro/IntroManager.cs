using System.Collections;
using UnityEngine;

public class IntroManager : MonoBehaviour
{
    #region serialized fields
    [SerializeField] GameObject cage;
    [SerializeField] GameObject ballObject;
    [SerializeField] CanvasGroup scoreCG;
    [SerializeField] CanvasGroup uiCG;
    [SerializeField] NPCNav anthonyNav;
    [SerializeField] float waitBeforeSpeaking = 2;

    [SerializeField] string[] dialogues;
    [SerializeField] GameObject dashAbilityPrefab;
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

        while (DialogueController.Instance.InDialogue)
        {
            yield return null;
        }

        DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);

        while (DialogueController.Instance.InDialogue || RewardWindow.Instance.InAbilitySelect)
        {
            yield return null;
        }

        DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);

        while (!InputManager.Instance.Ability0Action.IsPressed())
        {
            yield return null;
        }

        yield return new WaitForSeconds(waitBeforeSpeaking);
        DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);
    }

    public void GainDash()
    {
        RewardWindow.Instance.GiveReward(dashAbilityPrefab);
    }
}