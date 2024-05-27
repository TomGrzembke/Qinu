using System.Collections;
using UnityEngine;

public class BallTPSaveAbility : Ability
{
    #region serialized fields
    [SerializeField] float tpTime = .3f;
    [SerializeField] float spaceToAdd = 3;
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
        yield return new WaitForSeconds(tpTime);
        SlotManager.Puk.position = SlotManager.PlayerObj.position.Add(new(spaceToAdd, 0, 0));
        saveRoutine = null;
    }
}