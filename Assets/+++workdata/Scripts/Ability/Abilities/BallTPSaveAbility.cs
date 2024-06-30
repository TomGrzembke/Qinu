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
    BallVFX ballVFX;
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
        ballVFX = SlotManager.Puk.GetComponent<BallVFX>();
    }

    IEnumerator TPBall()
    {
        if (ballVFX)
            ballVFX.PlayTPVisual();

        yield return new WaitForSeconds(tpTime);

        if (ballVFX)
            ballVFX.PlayTPReachedVFX();

        SlotManager.Puk.position = SlotManager.PlayerObj.position.Add(new(spaceToAdd, 0, 0));
        saveRoutine = null;
    }
}