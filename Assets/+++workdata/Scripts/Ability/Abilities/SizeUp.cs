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
    FlipObjectOnVelocity flipObjectOnVelocity;
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
        flipObjectOnVelocity = SlotManager.PlayerObj.GetComponent<FlipObjectOnVelocity>();
    }

    IEnumerator TPBall()
    {
        float originalScale = flipObjectOnVelocity.MaxScale;
        flipObjectOnVelocity.SetMaxScale(flipObjectOnVelocity.MaxScale * timesSize);

        yield return new WaitForSeconds(duration);
        flipObjectOnVelocity.SetMaxScale(originalScale);
        saveRoutine = null;
    }
}