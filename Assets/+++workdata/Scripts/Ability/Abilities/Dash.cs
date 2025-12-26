using UnityEngine;

//The indiviual function for the current ability 
public class Dash : Ability
{
    #region Serialized

    #endregion

    #region Non Serialized
    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;
    MovePlayer moveRB;
    #endregion

    protected override void OnInitializedInternal()
    {
        moveRB = SlotManager.PlayerObj.GetComponent<MovePlayer>();
    }

    protected override void DeExecuteInternal()
    {

    }

    protected override void ExecuteInternal()
    {
        moveRB.Dash();
    }

}