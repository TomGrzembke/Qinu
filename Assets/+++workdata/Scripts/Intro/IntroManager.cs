using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
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
    [SerializeField] float playTimeBeforeGoalsOpen = 7;

    [SerializeField] string[] dialogues;
    [SerializeField] GameObject dashAbilityPrefab;
    [SerializeField] GameObject anthony;
    #endregion

    #region private fields
    bool IsPlaying => TournamentManager.Instance.GameState == TournamentManager.GameStateEnum.InGame;
    int dialogueID = -1;
    [SerializeField] bool skipTutPoint;
    Task task;
    #endregion

    async Task Start()
    {
        await IntroCoroutine();
        //yield return new WaitForSeconds(.1f);
        //StartCoroutine(IntroCoroutine());
        TournamentManager.Instance.LeftPlayerAdd();
        TournamentManager.Instance.RightPlayerAdd(anthony);
    }

    async Task IntroCoroutine()
    {
        
        await new WaitForSeconds(waitBeforeSpeaking);
        DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);

        await new WaitUntil(() => !DialogueController.Instance.InDialogue);

        DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);

        //Vector3 pukPos = MinigameManager.Instance.Puk.position;
        //while (MinigameManager.Instance.Puk.position.Equals(pukPos) && !TriggerSkipTutPoint())


        //yield return new WaitForSeconds(playTimeBeforeGoalsOpen);

        //DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);

        //while (DialogueController.Instance.InDialogue || RewardWindow.Instance.InAbilitySelect && !TriggerSkipTutPoint())
        {
            //yield return null;
        }

        //DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);

        //while (!InputManager.Instance.Ability0Action.IsPressed() && !TriggerSkipTutPoint())
        {
            //yield return null;
        }

        //DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);

        //while (IsPlaying && !TriggerSkipTutPoint())
        {
            //yield return null;
        }
        //DialogueController.Instance.StartDialogue(dialogues[++dialogueID]);
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