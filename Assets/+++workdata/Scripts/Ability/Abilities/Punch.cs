using UnityEngine;

public class Punch : Ability
{
    [SerializeField] GameObject punchPrefab;
    [SerializeField] float[] timePerRarity;


    PunchController punchController;
    GameObject punch;
    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;

    protected override void OnInitializedInternal()
    {

    }

    protected override void ExecuteInternal()
    {
        PunchStart();
    }


    void PunchStart()
    {
        punch = Instantiate(punchPrefab, SlotManager.PlayerObj.position, Quaternion.identity);

        var sizeUp = SlotManager.GetComponentInChildren<SizeUp>();
        
        if (sizeUp != null && sizeUp.IsSizeUpActive)
        {
            punch.transform.localScale *= sizeUp.GetCurrentSize();
        }

        // punch.transform.parent = SlotManager.PlayerObj.transform;
        punchController = punch.GetComponentInChildren<PunchController>();
        punchController.OnAttackFinished += PunchFinished;

        var time = timePerRarity[EvaluateRaritySizing(timePerRarity.Length)];

        punchController.SetAttackWindupTime(time);
        punchController.SetAttackWindupTime(time);
        punchController.SetPercentAlphaWindupAttackLock(time);
    }

    void PunchFinished()
    {
        punchController.OnAttackFinished -= PunchFinished;
        if (punch != null)
        {
            Destroy(punch);
        }
    }

    protected override void CleanupInternal()
    {
        if (punchController != null)
        {
            punchController.Stop();
            punchController.OnAttackFinished -= PunchFinished;

            punchController = null;
            punch = null;
        }

        if (punch != null)
        {
            Destroy(punch);
        }

        Clear();
    }
}