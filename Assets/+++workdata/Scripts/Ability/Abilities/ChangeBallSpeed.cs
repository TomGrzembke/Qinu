using System.Collections;
using UnityEngine;

public class ChangeBallSpeed : Ability
{
    #region serialized fields
    [SerializeField] float amount = 10;
    [SerializeField] float changedTime = 3;
    #endregion

    #region private fields
    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;
    Coroutine saveRoutine;
    #endregion

    protected override void DeExecuteInternal()
    {

    }

    protected override void ExecuteInternal()
    {
        if (saveRoutine == null)
            saveRoutine = StartCoroutine(TPBall());
    }

    protected override void OnInitializedInternal()
    {
    }

    IEnumerator TPBall()
    {
        SlotManager.Puk.GetComponent<BallController>().AddBallMaxSpeed(amount, true);
        yield return new WaitForSeconds(changedTime);
        SlotManager.Puk.GetComponent<BallController>().AddBallMaxSpeed(-amount, false);
        saveRoutine = null;
    }
}