using UnityEngine;

public class Dash : Ability
{
    #region serialized fields

    #endregion

    #region non serilized
    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;
    MoveRB moveRB;
    #endregion
    protected override void OnInitializedInternal()
    {
        moveRB = SlotManager.PlayerObj.GetComponent<MoveRB>();
    }

    protected override void DeExecuteInternal()
    {

    }

    protected override void ExecuteInternal()
    {
        moveRB.Dash();
    }

}