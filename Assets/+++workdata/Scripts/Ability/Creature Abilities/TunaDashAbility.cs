using System.Collections;
using UnityEngine;

public class TunaDashAbility : Ability
{
    #region serialized fields
    [SerializeField] float dashTime;
    #endregion

    #region private fields
    AbilitySlotManager slotManager;
    Coroutine dashRoutine;
    #endregion

    protected override void DeExecuteInternal()
    {

    }

    protected override void ExecuteInternal()
    {
        if(dashRoutine == null)
        dashRoutine = StartCoroutine(ApplySpeedUp());
    }

    protected override void OnInitializedInternal(AbilitySlotManager _abilitySlotManager)
    {
        slotManager = _abilitySlotManager;
        //statusManager = slotManager.StatusManager;
    }

    IEnumerator ApplySpeedUp()
    {
        //statusManager.SpeedSubject.AddSpeedModifier(speedModifier);
        yield return new WaitForSeconds(dashTime);
        //statusManager.SpeedSubject.RemoveSpeedModifier(speedModifier);
        dashRoutine = null;
    }
}