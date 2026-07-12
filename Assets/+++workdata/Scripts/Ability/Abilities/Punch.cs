using System.Collections;
using UnityEngine;

public class Punch : Ability
{
    [SerializeField] float punchTime = .3f;
    [SerializeField] GameObject punchPrefab;

    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;
    Coroutine abilityRoutine;


    protected override void OnInitializedInternal()
    {

    }

    protected override void ExecuteInternal()
    {
        if (abilityRoutine != null) return;

        abilityRoutine = StartCoroutine(PunchRoutine());
    }


    IEnumerator PunchRoutine()
    {
        GameObject punch = Instantiate(punchPrefab, SlotManager.PlayerObj.position, Quaternion.identity);
        punch.transform.parent = SlotManager.PlayerObj.transform;

        yield return new WaitForSeconds(punchTime);
        Destroy(punch);
        abilityRoutine = null;
    }

    protected override void CleanupInternal()
    {

    }
}