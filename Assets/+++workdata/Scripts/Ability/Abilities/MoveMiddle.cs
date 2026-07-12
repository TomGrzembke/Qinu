using System.Collections;
using UnityEngine;

public class MoveMiddle : Ability
{
    [SerializeField] float spaceToAdd = 3;

    [Header("Time")]
    [SerializeField] float movedTime = 4;
    [SerializeField] float timeToLerpTo = 1;
    [SerializeField] float timeToLerpBack = 3;

    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;
    Vector3 fromPos;
    Vector3 toPos;
    Coroutine abilityRoutine;

    protected override void ExecuteInternal()
    {
        if (abilityRoutine != null) return;

        abilityRoutine = StartCoroutine(TPBall());
    }

    protected override void OnInitializedInternal()
    {
        fromPos = SlotManager.MiddleStartPosition;
        toPos = SlotManager.MiddleStartPosition.Add(spaceToAdd, 0);
    }

    IEnumerator TPBall()
    {
        float lerpTime = 0;

        while (lerpTime < timeToLerpTo)
        {
            SlotManager.Middle.position = Vector3.Lerp(fromPos, toPos, lerpTime / timeToLerpTo);
            lerpTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(movedTime);

        lerpTime = 0;

        while (lerpTime < timeToLerpBack)
        {
            SlotManager.Middle.position = Vector3.Lerp(toPos, fromPos, lerpTime / timeToLerpBack);
            lerpTime += Time.deltaTime;
            yield return null;
        }

        abilityRoutine = null;
    }

    protected override void CleanupInternal()
    {
        // SlotManager.Middle.position = fromPos;
        //Fix Qinu being on the right and no snapback problem 
    }
}
