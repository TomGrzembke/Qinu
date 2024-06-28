using System.Collections;
using UnityEngine;

//The indiviual function for the current ability 
public class Punch : Ability
{
    #region serialized fields
    [SerializeField] float punchTime = .3f;
    [SerializeField] GameObject punchPrefab;
    #endregion

    #region private fields
    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;
    Coroutine abilityRoutine;
    #endregion

    protected override void DeExecuteInternal()
    {

    }

    protected override void ExecuteInternal()
    {
        if (abilityRoutine == null)
            abilityRoutine = StartCoroutine(PunchRoutine());
    }

    protected override void OnInitializedInternal()
    {

    }

    IEnumerator PunchRoutine()
    {
        GameObject punch = Instantiate(punchPrefab, SlotManager.PlayerObj.position, Quaternion.identity);
        punch.transform.parent = SlotManager.PlayerObj.transform;

        yield return new WaitForSeconds(punchTime);
        Destroy(punch);
        abilityRoutine = null;
    }
}