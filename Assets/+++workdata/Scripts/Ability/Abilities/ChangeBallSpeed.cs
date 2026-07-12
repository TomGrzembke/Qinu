using System.Collections;
using UnityEngine;

public class ChangeBallSpeed : Ability
{
    [SerializeField] float speedAmount = 10;
    [SerializeField] float duration = 3;

    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;
    Coroutine abilityRoutine;
    BallVFX ballVFX;
    BallController ballController;
    float currentGivenSpeedAmount;

    protected override void ExecuteInternal()
    {
        if (abilityRoutine != null) return;

        abilityRoutine = StartCoroutine(SpeedUpBall());
    }

    protected override void OnInitializedInternal()
    {
        ballController = SlotManager.Puk.GetComponent<BallController>();
        ballVFX = SlotManager.Puk.GetComponent<BallVFX>();
    }

    IEnumerator SpeedUpBall()
    {
        ballVFX.ChangeSprite(abilitySO.abilitySprite, duration);
        ballController.AddBallMaxSpeed(speedAmount, true);
        currentGivenSpeedAmount = speedAmount;

        yield return new WaitForSeconds(duration);

        ResetSpeed();
    }

    protected void ResetSpeed()
    {
        ballController.AddBallMaxSpeed(-currentGivenSpeedAmount, false);
        currentGivenSpeedAmount = 0;
        abilityRoutine = null;
    }

    protected override void CleanupInternal()
    {
        ResetSpeed();
    }
}