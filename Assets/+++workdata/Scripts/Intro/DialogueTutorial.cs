using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary> Uses a list of dialogue segements to create a custom dynamic sequence of dialogue and gameplay </summary>
public class DialogueTutorial : MonoBehaviour
{
    [SerializeField] List<DialogueSegment> dialogueSegment;

    [SerializeField] GameObject dashAbilityPrefab;
    [SerializeField] GameObject anthony;
    [SerializeField] InkEvents eventsOfTutorial;
    [SerializeField] AudioClip startMusic;

    bool IsPlaying => TournamentManager.Instance.GameState == TournamentManager.GameStateEnum.InGame;
    bool Ability0Pressed => AbilitySlotManager.Instance.GetAbilitySlotPerformed(0);
    Coroutine storySegmentCor;
    Vector3 pukPos;
    bool goalFreshlyShot;

    void Start()
    {
        if (startMusic)
        {
            SoundManager.Instance.PlayMusic(startMusic);
        }

        if (MinigameManager.Instance)
        {
            pukPos = MinigameManager.Instance.Puk.position;
            MinigameManager.OnGoalShot += OnGoalShot;
        }

        StartCoroutine(IntroCoroutine());

        TournamentManager.Instance.LeftPlayerAdd();

        if (anthony)
        {
            TournamentManager.Instance.RightPlayerAdd(anthony);
        }
    }

    void OnDestroy()
    {
        MinigameManager.OnGoalShot -= OnGoalShot;
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
                return Ability0Pressed;

            case ContineCondition.InRound:
                return !IsPlaying;

            case ContineCondition.WaitGoalShot:
                return goalFreshlyShot;
            default:
                return true;
        }
    }

    void OnGoalShot(Vector2 standing)
    {
        goalFreshlyShot = true;
        StartCoroutine(CleanupGoalShot());

        IEnumerator CleanupGoalShot()
        {
            yield return null;
            goalFreshlyShot = false;
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
    InRound,
    WaitGoalShot,
}

[Serializable]
public class DialogueSegment
{
    public float beforeWaitSeconds;
    public string dialogueName;
    public float afterWaitSeconds;
    public ContineCondition condition;
}