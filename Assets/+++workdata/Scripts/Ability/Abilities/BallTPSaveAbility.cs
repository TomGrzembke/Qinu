using System.Collections;
using UnityEngine;

public class BallTPSaveAbility : Ability
{
    [SerializeField] float[] tpTimePerRarity;

    [SerializeField] float spaceToAdd = 3;

    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;
    Coroutine tpRoutine;
    BallVFX ballVFX;

    protected override void ExecuteInternal()
    {
        if (tpRoutine != null) return;

        tpRoutine = StartCoroutine(TPBall());
    }

    protected override void OnInitializedInternal()
    {
        ballVFX = SlotManager.Puk.GetComponent<BallVFX>();
    }

    IEnumerator TPBall()
    {
        if (ballVFX)
        {
            ballVFX.PlayTPVisual();
        }

        yield return new WaitForSeconds(tpTimePerRarity[EvaluateRaritySizing(tpTimePerRarity.Length)]);

        if (ballVFX)
        {
            ballVFX.PlayTPReachedVFX();
        }

        SlotManager.Puk.position = SlotManager.PlayerObj.position.Add(new(spaceToAdd, 0, 0));
        tpRoutine = null;
    }

    protected override void CleanupInternal()
    {
        QueueDestroy(tpRoutine);
    }
}