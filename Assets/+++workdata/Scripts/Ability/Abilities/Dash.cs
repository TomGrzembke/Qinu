using UnityEngine;

public class Dash : Ability
{
    [SerializeField] float[] dashMultiplierPerRarity;

    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;
    MovePlayer moveRB;

    protected override void OnInitializedInternal()
    {
        moveRB = SlotManager.PlayerObj.GetComponent<MovePlayer>();
    }

    protected override void CleanupInternal()
    {
        Clear();
    }

    protected override void ExecuteInternal()
    {
        moveRB.Dash(dashMultiplierPerRarity[EvaluateRaritySizing(dashMultiplierPerRarity.Length)]);
    }

}