using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroManager : MonoBehaviour
{
    #region serialized fields
    [SerializeField] List<DialogueSegment> dialogueSegment;

    [SerializeField] GameObject cage;
    [SerializeField] GameObject ballObject;
    [SerializeField] CanvasGroup scoreCG;
    [SerializeField] CanvasGroup uiCG;
    [SerializeField] NPCNav anthonyNav;
    [SerializeField] float waitBeforeSpeaking = 2;
    [SerializeField] float playTimeBeforeGoalsOpen = 7;

    [SerializeField] string[] dialogues;
    [SerializeField] GameObject dashAbilityPrefab;
    [SerializeField] GameObject anthony;
    #endregion

    #region private fields
    bool IsPlaying => TournamentManager.Instance.GameState == TournamentManager.GameStateEnum.InGame;
    int dialogueID = -1;
    [SerializeField] bool skipTutPoint;
    #endregion

    void Start()
    {
        StartCoroutine(IntroCoroutine());
        TournamentManager.Instance.LeftPlayerAdd();
        TournamentManager.Instance.RightPlayerAdd(anthony);
    }

    IEnumerator IntroCoroutine()
    {
        yield return new WaitForSeconds(waitBeforeSpeaking);
        DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);

        yield return new WaitUntil(() => !DialogueController.Instance.InDialogue || TriggerSkipTutPoint());
        DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);

        Vector3 pukPos = MinigameManager.Instance.Puk.position;
        yield return new WaitUntil(() => !MinigameManager.Instance.Puk.position.Equals(pukPos) || TriggerSkipTutPoint());
        yield return new WaitForSeconds(playTimeBeforeGoalsOpen);
        DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);

        yield return new WaitUntil(() => !DialogueController.Instance.InDialogue && !RewardWindow.Instance.InAbilitySelect || TriggerSkipTutPoint());
        DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);

        yield return new WaitUntil(() => InputManager.Instance.Ability0Action.IsPressed() || TriggerSkipTutPoint());
        DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);

        yield return new WaitUntil(() => !IsPlaying || TriggerSkipTutPoint());
        DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);

    }

    IEnumerator StorySegmentCor(DialogueSegment dialogueSegment)
    {
        yield return new WaitForSeconds(dialogueSegment.beforeWaitSeconds);
        DialogueController.Instance.StartDialogue(dialogueSegment.dialogueName);
        yield return new WaitForSeconds(dialogueSegment.afterWaitSeconds);

        yield return new WaitUntil(() => CheckCondition(dialogueSegment));
    }

    bool CheckCondition(DialogueSegment dialogueSegment)
    {
        switch (dialogueSegment.condition)
        {
            case ContineCondition.DialogueSingle:
                return true;

            case ContineCondition.DialogueWait:
                return !DialogueController.Instance.InDialogue;

            case ContineCondition.WaitBallMove:
                Vector3 pukPos = MinigameManager.Instance.Puk.position;
                return !MinigameManager.Instance.Puk.position.Equals(pukPos);

            case ContineCondition.WaitAbilitySelect:
                return !RewardWindow.Instance.InAbilitySelect;

            case ContineCondition.ButtonPressed:
                return InputManager.Instance.Ability0Action.IsPressed();

            case ContineCondition.InRound:
                return !IsPlaying;

            default:
                return true;
        }
    }

    public void GainDash()
    {
        RewardWindow.Instance.GiveReward(dashAbilityPrefab);
    }

    bool TriggerSkipTutPoint()
    {
        if (skipTutPoint)
        {
            skipTutPoint = false;
            return true;
        }

        return false;
    }

    public void SetSkipTutPoint()
    {
        skipTutPoint = true;
    }
}

public enum ContineCondition
{
    DialogueSingle,
    DialogueWait,
    WaitBallMove,
    WaitAbilitySelect,
    ButtonPressed,
    InRound
}

[Serializable]
public class DialogueSegment
{
    public float beforeWaitSeconds;
    public string dialogueName;
    public float afterWaitSeconds;
    public ContineCondition condition;
}