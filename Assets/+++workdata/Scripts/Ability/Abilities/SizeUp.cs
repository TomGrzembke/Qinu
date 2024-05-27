using System.Collections;
using UnityEngine;

public class SizeUp : Ability
{
    #region serialized fields
    [SerializeField] float timesSize = 1.3f;
    [SerializeField] float duration = 3;
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
        Vector3 originalScale = SlotManager.PlayerObj.localScale;
        SlotManager.PlayerObj.localScale = SlotManager.PlayerObj.localScale * timesSize;
        yield return new WaitForSeconds(duration);
        SlotManager.PlayerObj.localScale = originalScale;
        saveRoutine = null;
    }
}