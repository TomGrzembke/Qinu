using UnityEngine;

public class DebugPanelUtility : MonoBehaviour
{
    public void WinRound()
    {
        TournamentManager.Instance.WinRound();
    }

    public void FinishTutorial()
    {
        var dialogueTutorials = FindObjectsByType<DialogueTutorial>();

        foreach(var tutorial in dialogueTutorials)
        {
           tutorial.SkipTutorial(); 
        }
    }

    public void AbilityReward()
    {
        RewardWindow.Instance.GiveReward();
        InputManager.Instance.ShowCursor();
    }

    public void SetGoalAmount(int goalAmount)
    {
        MinigameManager.Instance.SetPointsTilWin(goalAmount);
    }

    public void BossPrompt()
    {
        OptionalBossFightManager.Instance.ShowChoice();
        InputManager.Instance.ShowCursor();
    }
}
