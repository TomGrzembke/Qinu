using UnityEngine;

public class Dash : Ability
{
    #region serialized fields

    #endregion

    #region non serilized
    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;

    #endregion
    protected override void OnInitializedInternal()
    {
       
        //InputManager.Instance.SubscribeTo(SlotManager.PlayerObj.GetComponent<MoveRB>().Dash, InputManager.Instance.);
    }

    protected override void DeExecuteInternal()
    {

    }

    protected override void ExecuteInternal()
    {
        SlotManager.PlayerObj.GetComponent<MoveRB>().Dash();
    }

}