using System.Collections;
using UnityEngine;

//The indiviual function for the current ability 
public class BallTPSaveAbility : Ability
{
    #region Serialized
    [SerializeField] float tpTime = .3f;
    [SerializeField] float spaceToAdd = 3;
    #endregion

    #region Non Serialized
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