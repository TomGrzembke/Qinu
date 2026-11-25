using UnityEngine;

public class DebugPanelUtility : MonoBehaviour
{
    public void WinRound()
    {
        TournamentManager.Instance.WinRound();
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
