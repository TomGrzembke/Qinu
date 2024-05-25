using System.Collections;
using UnityEngine;

public class Punch : Ability
{
    #region serialized fields
    [SerializeField] float punchTime = .3f;
    [SerializeField] float spaceToPunch = 3;
    [SerializeField] GameObject punchPrefab;
    #endregion

    #region private fields
    AbilitySlotManager slotManager;
    Coroutine punchRoutine;
    #endregion

    protected override void DeExecuteInternal()
    {

    }

    protected override void ExecuteInternal()
    {
        if (punchRoutine == null)
            punchRoutine = StartCoroutine(PunchRoutine());
    }

    protected override void OnInitializedInternal(AbilitySlotManager _abilitySlotManager)
    {
        slotManager = _abilitySlotManager;
    }

    IEnumerator PunchRoutine()
    {
        GameObject punch = Instantiate(punchPrefab, slotManager.PlayerObj.position, Quaternion.identity);
        punch.transform.parent = slotManager.PlayerObj.transform;
        yield return new WaitForSeconds(punchTime);
        Destroy(punch);
        punchRoutine = null;
    }
}