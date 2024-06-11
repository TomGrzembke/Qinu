using UnityEngine;

public class Dash : Ability
{
    #region serialized fields

    #endregion

    #region non serilized
    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;

    #endregion

    protected override void DeExecuteInternal()
    {
        throw new System.NotImplementedException();
    }

    protected override void ExecuteInternal()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnInitializedInternal()
    {
        throw new System.NotImplementedException();
    }
}