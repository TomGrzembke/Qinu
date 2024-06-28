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
    #endregion

    protected override void DeExecuteInternal()
    {

    }

    protected override void ExecuteInternal()
    {
        if (abilityRoutine == null)
            abilityRoutine = StartCoroutine(TPBall());
    }

    protected override void OnInitializedInternal()
    {
    }

    IEnumerator TPBall()
    {
        SlotManager.Puk.GetComponent<BallController>().AddBallMaxSpeed(speedAmount, true);
        yield return new WaitForSeconds(duration);
        SlotManager.Puk.GetComponent<BallController>().AddBallMaxSpeed(-speedAmount, false);
        abilityRoutine = null;
    }
}