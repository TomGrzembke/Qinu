using System.Collections;
using UnityEngine;

//The indiviual function for the current ability 
public class ChangeBallSpeed : Ability
{
    #region Serialized
    [SerializeField] float speedAmount = 10;
    [SerializeField] float duration = 3;
    #endregion

    #region Non Serialized
    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;
    Coroutine abilityRoutine;
    BallVFX ballVFX;
    BallController ballController;
    #endregion

    protected override void DeExecuteInternal()
    {

    }

    protected override void ExecuteInternal()
    {
        if (abilityRoutine == null)
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
        yield return new WaitForSeconds(duration);
        ballController.AddBallMaxSpeed(-speedAmount, false);
        abilityRoutine = null;
    }
}