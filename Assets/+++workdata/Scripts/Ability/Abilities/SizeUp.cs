using System.Collections;
using UnityEngine;

public class SizeUp : Ability
{
    [SerializeField] float timesSize = 1.3f;
    [SerializeField] float duration = 3;

    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;
    FlipObjectOnVelocity flipObjectOnVelocity;
    Coroutine abilityRoutine;

    protected override void OnInitializedInternal()
    {
        flipObjectOnVelocity = SlotManager.PlayerObj.GetComponent<FlipObjectOnVelocity>();
    }

    protected override void ExecuteInternal()
    {
        if (abilityRoutine != null) return;
        
        abilityRoutine = StartCoroutine(SizeUpRoutine());
    }


    IEnumerator SizeUpRoutine()
    {
        float originalScale = flipObjectOnVelocity.MaxScale;
        flipObjectOnVelocity.SetMaxScale(flipObjectOnVelocity.MaxScale * timesSize);

        yield return new WaitForSeconds(duration);
        
        flipObjectOnVelocity.SetMaxScale(originalScale);
        abilityRoutine = null;
    }

    protected override void CleanupInternal()
    {
        QueueDestroy(abilityRoutine);

    }
}