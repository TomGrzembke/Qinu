using MyBox;
using TMPro;
using UnityEngine;

public class ChangeGoalAmount : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI goalText;
    [SerializeField] string goalMessage = "Match Goals: ";
    public void SetGoalAmount(float goalAmount)
    {
        int newGoals = goalAmount.RoundToInt();
        MinigameManager.Instance.SetPointsTilWin(newGoals);
        goalText.text = goalMessage + newGoals;
    }
}
