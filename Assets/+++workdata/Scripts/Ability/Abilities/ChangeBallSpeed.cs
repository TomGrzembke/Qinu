using System.Collections;
using UnityEngine;

public class ChangeBallSpeed : Ability
{
    [SerializeField] float[] speedPerRarity;
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
        var currentSpeedAmount = speedPerRarity[EvaluateRaritySizing(speedPerRarity.Length)];

        ballController.AddBallMaxSpeed(currentSpeedAmount, true);
        currentGivenSpeedAmount = currentSpeedAmount;

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
        QueueDestroy(abilityRoutine);
    }
}