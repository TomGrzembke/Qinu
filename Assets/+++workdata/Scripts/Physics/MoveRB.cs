using UnityEngine;
using UnityEngine.AI;

/// <summary> Either depending on agent provided or Input in case of the player</summary>
[RequireComponent(typeof(DashController))]
public class MoveRB : RBGetter
{
    const float arrivalTolerance = 0.05f;

    [SerializeField] NavMeshAgent agent;

    NPCRigidSettings CharSettings => charSO.CharSettings.CharRigidSettings;

    DashController dashController;
    NPCNav npcNavigation;
    CharSOHolder charSOHolder;
    NPCCharSO charSO;

    public Vector2 GetMoveDir()
    {
        if (agent == null) return Vector2.zero;

        return agent.desiredVelocity.RemoveZ().normalized;
    }

    protected override void AwakeInternal()
    {
        charSOHolder = GetComponent<CharSOHolder>();
        charSO = (NPCCharSO)charSOHolder.CharSO;
        charSOHolder.CharSOChanged += OnCharSOChanged;
        dashController = GetComponent<DashController>();
        npcNavigation = GetComponent<NPCNav>();

        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        UpdateAgentSettings();
    }

    void OnDisable()
    {
        rb.linearVelocity = Vector3.zero;
    }

    void OnDestroy()
    {
        if (charSOHolder)
        {
            charSOHolder.CharSOChanged -= OnCharSOChanged;
        }
    }

    void OnCharSOChanged(CharSO newCharSO)
    {
        if (newCharSO is not NPCCharSO newNPCCharSO)
        {
            Debug.LogError($"{name} requires NPCCharSO settings.", this);
            return;
        }

        charSO = newNPCCharSO;
        UpdateAgentSettings();
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, GetMaximumMoveSpeed());
    }

    void FixedUpdate()
    {
        UpdateAgentSettings();

        if (dashController.IsDashing) return;

        if (!agent.isOnNavMesh) return;

        agent.nextPosition = rb.position;

        float maximumMoveSpeed = GetMaximumMoveSpeed();
        Vector2 desiredVelocity = Vector2.ClampMagnitude(agent.desiredVelocity.RemoveZ(), maximumMoveSpeed);
        Vector2 velocityDifference = desiredVelocity - rb.linearVelocity;

        if (!agent.pathPending && (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + arrivalTolerance))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 correctionAcceleration = velocityDifference * CharSettings.VelocityCorrection;
        correctionAcceleration = Vector2.ClampMagnitude(correctionAcceleration, CharSettings.MaxCorrectionAcceleration);

        rb.AddForce(correctionAcceleration * rb.mass, ForceMode2D.Force);

        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maximumMoveSpeed);
    }

    void UpdateAgentSettings()
    {
        agent.speed = GetMaximumMoveSpeed();
        agent.acceleration = CharSettings.Acceleration / rb.mass;
    }

    float GetMaximumMoveSpeed()
    {
        float baseMaximumMoveSpeed = npcNavigation
            ? npcNavigation.GetBaseMaximumMoveSpeed()
            : CharSettings.MiddleLineMaxSpeed;

        return baseMaximumMoveSpeed * dashController.MoveSpeedMultiplier;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
    }
}
