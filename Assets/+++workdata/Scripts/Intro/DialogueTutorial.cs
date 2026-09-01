using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Uses a list of dialogue segements to create a custom dynamic sequence of dialogue and gameplay </summary>
public class DialogueTutorial : MonoBehaviour
{
    [SerializeField] List<DialogueSegment> dialogueSegment;

    [Header("References")]
    [SerializeField] GameObject dashAbilityPrefab;
    [SerializeField] GameObject anthony;

    [SerializeField] AudioClip startMusic;

    [Header("Ink Events")]
    [SerializeField] InkEvents eventsOfTutorial;

    bool IsPlaying => TournamentManager.Instance.GameState == TournamentManager.GameStateEnum.InGame;
    bool BaseAbilityPressed => AbilitySlotManager.Instance.GetAbilitySlotPerformed(0);
    Coroutine storySegmentRoutine;
    Vector3 pukPosoitionCache;
    bool isGoalShotThisFrame;

    void Start()
    {
        InitializeMusic();

        InitializeGoalShotListener();

        StartCoroutine(IntroCoroutine());

        AddNPCs();
    }

    void AddNPCs()
    {
        TournamentManager.Instance.LeftPlayerAdd();

        AddStartNPC();
    }

    void InitializeGoalShotListener()
    {
        if (MinigameManager.Instance == null) return;

        pukPosoitionCache = MinigameManager.Instance.Puk.position;
        MinigameManager.OnGoalShot += OnGoalShot;
    }

    void OnDestroy()
    {
        MinigameManager.OnGoalShot -= OnGoalShot;
    }

    private void AddStartNPC()
    {
        if (anthony == null) return;

        TournamentManager.Instance.RightPlayerAdd(anthony);
    }

    void InitializeMusic()
    {
        if (startMusic == null) return;

        SoundManager.Instance.PlayMusic(startMusic);
    }


    IEnumerator IntroCoroutine()
    {
        for (int i = 0; i < dialogueSegment.Count; i++)
        {
            yield return new WaitUntil(() => storySegmentRoutine == null);
            storySegmentRoutine = StartCoroutine(StorySegmentCor(dialogueSegment[i]));
        }
    }

    IEnumerator StorySegmentCor(DialogueSegment dialogueSegment)
    {
        yield return new WaitForSeconds(dialogueSegment.beforeWaitSeconds);

        DialogueController.Instance.StartDialogue(dialogueSegment.dialogueName);

        yield return new WaitForSeconds(dialogueSegment.afterWaitSeconds);

        yield return new WaitUntil(() => CheckCondition(dialogueSegment));
        storySegmentRoutine = null;
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
                return Vector3.Distance(pukPosoitionCache, MinigameManager.Instance.Puk.position) > 1;

            case ContineCondition.WaitAbilitySelect:
                if (DialogueController.Instance.InDialogue) return false;
                return !RewardWindow.Instance.InAbilitySelect;

            case ContineCondition.ButtonPressed:
                return BaseAbilityPressed;

            case ContineCondition.InRound:
                return !IsPlaying;

            case ContineCondition.WaitGoalShot:
                return isGoalShotThisFrame;
            default:
                return true;
        }
    }

    void OnGoalShot(Vector2 standing)
    {
        isGoalShotThisFrame = true;
        StartCoroutine(CleanupGoalShot());

        IEnumerator CleanupGoalShot()
        {
            yield return null;
            isGoalShotThisFrame = false;
        }
    }

    public void GainDash()
    {
        RewardWindow.Instance.GiveSingleReward(dashAbilityPrefab);
    }

    public void SkipTutorial()
    {
        StopCoroutine(storySegmentRoutine);
        eventsOfTutorial.InvokeAllEvents();
    }
}