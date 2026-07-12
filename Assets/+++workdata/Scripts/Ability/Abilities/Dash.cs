public class Dash : Ability
{
    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;
    MovePlayer moveRB;

    protected override void OnInitializedInternal()
    {
        moveRB = SlotManager.PlayerObj.GetComponent<MovePlayer>();
    }

    protected override void CleanupInternal()
    {

    }

    protected override void ExecuteInternal()
    {
        moveRB.Dash();
    }

}