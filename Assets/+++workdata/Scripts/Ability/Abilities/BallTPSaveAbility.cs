using System.Collections;
using UnityEngine;

public class BallTPSaveAbility : Ability
{
    [SerializeField] float[] tpTimePerRarity;

    [SerializeField] float spaceToAdd = 3;

    AbilitySlotManager SlotManager => AbilitySlotManager.Instance;
    Coroutine tpRoutine;
    BallVFX ballVFX;
    Rigidbody2D pukRB;
    SpriteRenderer pukRenderer;

    protected override void ExecuteInternal()
    {
        if (tpRoutine != null) return;

        tpRoutine = StartCoroutine(TPBall());
    }

    protected override void OnInitializedInternal()
    {
        ballVFX = SlotManager.Puk.GetComponent<BallVFX>();
        pukRB = SlotManager.Puk.GetComponent<Rigidbody2D>();
        pukRenderer = SlotManager.Puk.GetComponent<SpriteRenderer>();
    }

    IEnumerator TPBall()
    {
        Vector2 velocityAfterTeleport = IsMovingTowardPlayerGoal(pukRB.linearVelocity) ? Vector2.zero : pukRB.linearVelocity;

        if (ballVFX)
        {
            ballVFX.PlayTPVisual();
        }

        pukRenderer.enabled = false;
        pukRB.linearVelocity = Vector2.zero;

        float invisibleTime = tpTimePerRarity[EvaluateRaritySizing(tpTimePerRarity.Length)];
        float elapsedTime = 0f;

        while (elapsedTime < invisibleTime)
        {
            pukRB.linearVelocity = Vector2.zero;
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Vector3 teleportPosition = SlotManager.PlayerObj.position.Add(new(spaceToAdd, 0, 0));
        SlotManager.Puk.position = teleportPosition;
        pukRB.linearVelocity = velocityAfterTeleport;
        pukRenderer.enabled = true;

        if (ballVFX)
        {
            ballVFX.PlayTPReachedVFX();
        }

        tpRoutine = null;
    }

    bool IsMovingTowardPlayerGoal(Vector2 velocity)
    {
        if (velocity.sqrMagnitude <= Mathf.Epsilon) return false;

        bool hasLeftGoal = MinigameManager.Instance.TryGetGoalMiddle(false, out Vector2 leftGoal);
        bool hasRightGoal = MinigameManager.Instance.TryGetGoalMiddle(true, out Vector2 rightGoal);
        if (!hasLeftGoal && !hasRightGoal) return false;

        Vector2 playerPosition = SlotManager.PlayerObj.position;
        Vector2 ownGoal = leftGoal;
        bool rightGoalIsCloser = !hasLeftGoal || hasRightGoal && Vector2.SqrMagnitude(playerPosition - rightGoal) < Vector2.SqrMagnitude(playerPosition - leftGoal);

        if (rightGoalIsCloser)
        {
            ownGoal = rightGoal;
        }

        Vector2 directionToOwnGoal = ownGoal - SlotManager.Puk.position.RemoveZ();
        return Vector2.Dot(velocity, directionToOwnGoal) > 0f;
    }

    protected override void CleanupInternal()
    {
        QueueDestroy(tpRoutine);
    }
}
