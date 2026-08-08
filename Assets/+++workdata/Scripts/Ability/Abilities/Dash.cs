using UnityEngine;

public class Dash : Ability
{
    [SerializeField] float[] dashMultiplierPerRarity;

    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;
    DashController dashController;

    protected override void OnInitializedInternal()
    {
        dashController = SlotManager.PlayerObj.GetComponent<DashController>();
    }

    protected override void CleanupInternal()
    {
        Clear();
    }

    protected override void ExecuteInternal()
    {
        dashController.Dash(dashMultiplierPerRarity[EvaluateRaritySizing(dashMultiplierPerRarity.Length)]);
    }

}
