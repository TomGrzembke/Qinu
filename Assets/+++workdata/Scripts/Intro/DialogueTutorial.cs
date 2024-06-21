using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTutorial : MonoBehaviour
{
    #region serialized fields
    [SerializeField] List<DialogueSegment> dialogueSegment;

    [SerializeField] GameObject dashAbilityPrefab;
    [SerializeField] GameObject anthony;
    [SerializeField] InkEvents eventsOfTutorial;
    #endregion

    #region private fields
    bool IsPlaying => TournamentManager.Instance.GameState == TournamentManager.GameStateEnum.InGame;
    Coroutine storySegmentCor;
    Vector3 pukPos;

    #endregion

    void Start()
    {
        pukPos = MinigameManager.Instance.Puk.position;

        StartCoroutine(IntroCoroutine());
        TournamentManager.Instance.LeftPlayerAdd();
        TournamentManager.Instance.RightPlayerAdd(anthony);
    }

    IEnumerator IntroCoroutine()
    {
        for (int i = 0; i < dialogueSegment.Count; i++)
        {
            yield return new WaitUntil(() => storySegmentCor == null);
            storySegmentCor = StartCoroutine(StorySegmentCor(dialogueSegment[i]));
        }
    }

    IEnumerator StorySegmentCor(DialogueSegment dialogueSegment)
    {
        yield return new WaitForSeconds(dialogueSegment.beforeWaitSeconds);
        DialogueController.Instance.StartDialogue(dialogueSegment.dialogueName);
        yield return new WaitForSeconds(dialogueSegment.afterWaitSeconds);

        yield return new WaitUntil(() => CheckCondition(dialogueSegment));
        storySegmentCor = null;
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
                if (DialogueController.Instance.InDialogue) return false;
                return Vector3.Distance(pukPos, MinigameManager.Instance.Puk.position) > 1;

            case ContineCondition.WaitAbilitySelect:
                if (DialogueController.Instance.InDialogue) return false;
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
        RewardWindow.Instance.GiveSingleReward(dashAbilityPrefab);
    }

    public void SkipTutorial()
    {
        StopCoroutine(storySegmentCor);
        eventsOfTutorial.InvokeAllEvents();
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