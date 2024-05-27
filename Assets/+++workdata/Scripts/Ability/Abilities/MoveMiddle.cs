using System.Collections;
using UnityEngine;

public class MoveMiddle : Ability
{
    #region serialized fields
    [SerializeField] float spaceToAdd = 3;

    [Header("Time")]
    [SerializeField] float movedTime = 4;
    [SerializeField] float timeToLerpTo = 1;
    [SerializeField] float timeToLerpBack = 3;
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
        Vector3 fromPos = SlotManager.Middle.position;
        Vector3 toPos = SlotManager.Middle.position.Add(spaceToAdd, 0);

        float lerpTime = 0;

        while (lerpTime < timeToLerpTo)
        {
            SlotManager.Middle.position = Vector3.Lerp(fromPos, toPos, (lerpTime / timeToLerpTo));
            lerpTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(movedTime);

        lerpTime = 0;

        while (lerpTime < timeToLerpBack)
        {
            SlotManager.Middle.position = Vector3.Lerp(toPos, fromPos, (lerpTime / timeToLerpBack));
            lerpTime += Time.deltaTime;
            yield return null;
        }

        saveRoutine = null;

    }
}
