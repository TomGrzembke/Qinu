using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary> Intends to achieve fluid player movement via smoothing </summary>
[RequireComponent(typeof(DashController))]
public class MovePlayer : RBGetter
{
    readonly struct MovementCastConstraint
    {
        public readonly Vector2 Normal;
        public readonly float MaximumInwardSpeed;

        public MovementCastConstraint(Vector2 normal, float maximumInwardSpeed)
        {
            Normal = normal;
            MaximumInwardSpeed = maximumInwardSpeed;
        }
    }

    const float PreviousPhysicsStep = 0.02f;
    const int MovementCastHitCapacity = 8;

    [SerializeField] bool disableInputRightclick;
    [SerializeField] Transform virtualMouseDebug;
    [SerializeField, Min(0f)] float collisionNormalRetentionTime = 0.03f;
    [SerializeField] Collider2D movementCollider;
    [SerializeField, Min(0f)] float movementCastSkin = 0.05f;

    [Header("High-speed puck collider")]
    [SerializeField, Range(0f, 1f)] float extraColliderEnableSpeedRatio = 0.85f;
    [SerializeField, Range(0f, 1f)] float extraColliderDisableSpeedRatio = 0.7f;

    AnimationCurve moveCurve => charSO.CharSettings.CharRigidSettings.MoveCurve;
    float maxSpeedDistance => charSO.CharSettings.CharRigidSettings.MaxSpeedDistance;
    float maxSpeed => charSO.CharSettings.CharRigidSettings.MaxSpeed;
    float minSpeed => charSO.CharSettings.CharRigidSettings.MinSpeed;
    float stoppingDistance => charSO.CharSettings.CharRigidSettings.StoppingDistance;
    float acceleration => charSO.CharSettings.CharRigidSettings.Acceleration;
    float turningResponse => charSO.CharSettings.CharRigidSettings.TurningResponse;
    float decceleration => charSO.CharSettings.CharRigidSettings.Decceleration;
    bool inputDisabled;
    float currentMaxSpeed;
    DashController dashController;

    PlayerCharSO charSO;
    Vector2 virtualMouseOffset;
    Vector2 currentMoveDirection;
    bool isReturningFromDash;

    readonly List<Vector2> cachedDirections = new();
    readonly Dictionary<Collider2D, Vector2> solidCollisionNormals = new();
    readonly Dictionary<Collider2D, float> collisionNormalExpiryTimes = new();
    readonly List<Collider2D> expiredCollisionNormals = new();
    readonly List<MovementCastConstraint> movementCastConstraints = new();
    readonly RaycastHit2D[] movementCastHits = new RaycastHit2D[MovementCastHitCapacity];
    ContactFilter2D movementCastFilter;

    [SerializeField] int cachedDirectionAmount = 5;
    [SerializeField, Range(0f, 1f)] float maxCachedDirectionPercentage = 0.4f;

    [SerializeField] private Collider2D extraBallCollider;

    Camera Cam;
    public Vector2 DashAimPosition => GetVirtualMousePosition();

    Camera GetCam()
    {
        if (Cam == null) Cam = Camera.main;

        return Cam;
    }

    protected override void AwakeInternal()
    {
        charSO = (PlayerCharSO)GetComponent<CharSOHolder>().CharSO;
        dashController = GetComponent<DashController>();
        currentMaxSpeed = maxSpeed;
        ConfigureMovementCastFilter();

        if (disableInputRightclick)
        {
            InputManager.Instance.SubscribeTo(DisableInput, InputManager.Instance.RightClickAction);
        }
    }

    void OnEnable()
    {
        dashController.DashStarted += OnDashStarted;
    }

    void OnDisable()
    {
        if (dashController)
        {
            dashController.DashStarted -= OnDashStarted;
        }

        if (disableInputRightclick)
        {
            InputManager.Instance.DesubscribeTo(DisableInput, InputManager.Instance.RightClickAction);
        }

        solidCollisionNormals.Clear();
        collisionNormalExpiryTimes.Clear();
        expiredCollisionNormals.Clear();
        movementCastConstraints.Clear();
        isReturningFromDash = false;
        rb.linearVelocity = Vector3.zero;
    }

    void OnValidate()
    {
        cachedDirectionAmount = Mathf.Max(1, cachedDirectionAmount);
        collisionNormalRetentionTime = Mathf.Max(0f, collisionNormalRetentionTime);
        movementCastSkin = Mathf.Max(0f, movementCastSkin);
        extraColliderDisableSpeedRatio = Mathf.Min(extraColliderDisableSpeedRatio, extraColliderEnableSpeedRatio);
    }

    Vector2 SampleMoveDirection()
    {
        if (inputDisabled) return ResetMoveDirection();

        Vector2 rawDirection = GetRawDirection(GetVirtualMousePosition());
        float cursorDistance = rawDirection.magnitude;

        bool isInsideStoppingDistance = rawDirection.sqrMagnitude <= stoppingDistance * stoppingDistance;
        if (isInsideStoppingDistance) return ResetMoveDirection();

        Vector2 currentDirection = rawDirection / cursorDistance;
        cachedDirections.Add(currentDirection);

        if (cachedDirections.Count > cachedDirectionAmount)
        {
            cachedDirections.RemoveAt(0);
        }

        Vector2 smoothedDirection = GetSmoothedDirection(currentDirection);

        return smoothedDirection * cursorDistance;
    }

    Vector2 ResetMoveDirection()
    {
        cachedDirections.Clear();

        return Vector2.zero;
    }

    Vector2 GetSmoothedDirection(Vector2 currentDirection)
    {
        if (cachedDirections.Count <= 1) return currentDirection;

        Vector2 weightedDirection = Vector2.zero;
        float totalWeight = 0f;

        for (int i = 0; i < cachedDirections.Count; i++)
        {
            float weight = i + 1f;
            weightedDirection += cachedDirections[i] * weight;
            totalWeight += weight;
        }

        if (weightedDirection.sqrMagnitude <= Mathf.Epsilon) return currentDirection;

        Vector2 averageDirection = (weightedDirection / totalWeight).normalized;
        float historicalInfluence = Mathf.Min(GetMouseDistanceAlpha(), maxCachedDirectionPercentage);
        Vector2 smoothedDirection = Vector2.Lerp(currentDirection, averageDirection, historicalInfluence);

        return smoothedDirection.normalized;
    }

    float GetMouseDistanceAlpha()
    {
        float distance = Vector2.Distance(transform.position, GetVirtualMousePosition());
        float distanceAlpha = Mathf.Clamp01(distance / maxSpeedDistance);
        return moveCurve.Evaluate(distanceAlpha);
    }

    void FixedUpdate()
    {
        if (dashController.IsDashing) return;

        RemoveExpiredCollisionNormals();
        ResetVirtualCursorOffsetWhenVisible();

        if (isReturningFromDash)
        {
            UpdateDashReturnTarget();
        }
        else
        {
            ConstrainVirtualCursor();
        }

        VirtualCursorDebug();

        currentMoveDirection = SampleMoveDirection();
        CalculateMaxSpeed();
        UpdateMovementCastNormals(currentMoveDirection);
        UpdateVelocity(currentMoveDirection);

        ClampVelocity();
        UpdateExtraBallCollider();
    }

    void VirtualCursorDebug()
    {
        if (virtualMouseDebug == null) return;

        virtualMouseDebug.position = GetVirtualMousePosition();
    }

    void UpdateVelocity(Vector2 moveDirection)
    {
        Vector2 currentVelocity = RemoveBlockedMovement(rb.linearVelocity);
        Vector2 desiredVelocity = GetDesiredVelocity(moveDirection);

        if (desiredVelocity == Vector2.zero)
        {
            float previousStepVelocityMultiplier = Mathf.Max(0f, 1f - decceleration * PreviousPhysicsStep);
            float stepRatio = Time.fixedDeltaTime / PreviousPhysicsStep;
            float velocityMultiplier = Mathf.Pow(previousStepVelocityMultiplier, stepRatio);
            currentVelocity *= velocityMultiplier;

            if (currentVelocity.magnitude < 0.01f) currentVelocity = Vector2.zero;
        }
        else
        {
            float velocityResponse = IsChangingDirection(currentVelocity, desiredVelocity) ? turningResponse : acceleration;
            float maximumVelocityChange = velocityResponse * Time.fixedDeltaTime;
            currentVelocity = Vector2.MoveTowards(currentVelocity, desiredVelocity, maximumVelocityChange);
        }

        rb.linearVelocity = currentVelocity;
    }

    Vector2 GetDesiredVelocity(Vector2 moveDirection)
    {
        if (moveDirection.sqrMagnitude <= Mathf.Epsilon) return Vector2.zero;

        Vector2 desiredVelocity = moveDirection.normalized * currentMaxSpeed;
        return RemoveBlockedMovement(desiredVelocity);
    }

    bool IsChangingDirection(Vector2 currentVelocity, Vector2 desiredVelocity)
    {
        if (currentVelocity.sqrMagnitude <= Mathf.Epsilon) return false;

        float directionAlignment = Vector2.Dot(currentVelocity.normalized, desiredVelocity.normalized);
        return directionAlignment < 0.99f;
    }

    void UpdateExtraBallCollider()
    {
        if (!extraBallCollider) return;

        float speedRatio = maxSpeed <= Mathf.Epsilon ? 0f : rb.linearVelocity.magnitude / maxSpeed;

        if (!extraBallCollider.enabled && speedRatio >= extraColliderEnableSpeedRatio)
        {
            extraBallCollider.enabled = true;
        }
        else if (extraBallCollider.enabled && speedRatio <= extraColliderDisableSpeedRatio)
        {
            extraBallCollider.enabled = false;
        }
    }

    void ResetVirtualCursorOffsetWhenVisible()
    {
        if (virtualMouseOffset == Vector2.zero) return;
        if (Cursor.lockState == CursorLockMode.Locked) return;

        virtualMouseOffset = Vector2.zero;
    }

    void ConstrainVirtualCursor()
    {
        var cursorDistance = maxSpeedDistance;
        if (cursorDistance <= 0f) return;

        Vector2 playerPosition = transform.position.RemoveZ();
        Vector2 virtualCursorPosition = GetVirtualMousePosition();
        Vector2 playerToCursor = virtualCursorPosition - playerPosition;

        if (playerToCursor.sqrMagnitude <= cursorDistance * cursorDistance) return;

        Vector2 constrainedCursorPosition = playerPosition + playerToCursor.normalized * cursorDistance;
        virtualMouseOffset += constrainedCursorPosition - virtualCursorPosition;
    }

    void CalculateMaxSpeed()
    {
        float configuredMoveSpeed = Mathf.Lerp(minSpeed, maxSpeed, GetMouseDistanceAlpha());
        currentMaxSpeed = configuredMoveSpeed * dashController.MoveSpeedMultiplier;
    }

    Vector2 RemoveBlockedMovement(Vector2 moveDirection)
    {
        foreach (Vector2 collisionNormal in solidCollisionNormals.Values)
        {
            moveDirection = RemoveMovementIntoNormal(moveDirection, collisionNormal);
        }

        foreach (MovementCastConstraint castConstraint in movementCastConstraints)
        {
            moveDirection = LimitMovementByCast(moveDirection, castConstraint);
        }

        return moveDirection;
    }

    Vector2 LimitMovementByCast(Vector2 moveDirection, MovementCastConstraint castConstraint)
    {
        float inwardSpeed = Vector2.Dot(moveDirection, castConstraint.Normal);
        float minimumAllowedInwardSpeed = -castConstraint.MaximumInwardSpeed;

        if (inwardSpeed < minimumAllowedInwardSpeed)
        {
            moveDirection += castConstraint.Normal * (minimumAllowedInwardSpeed - inwardSpeed);
        }

        return moveDirection;
    }

    Vector2 RemoveMovementIntoNormal(Vector2 moveDirection, Vector2 collisionNormal)
    {
        float movementIntoObstacle = Vector2.Dot(moveDirection, collisionNormal);
        bool isMovingIntoObstacle = movementIntoObstacle < 0f;

        if (isMovingIntoObstacle)
        {
            moveDirection -= collisionNormal * movementIntoObstacle;
        }

        return moveDirection;
    }

    void ConfigureMovementCastFilter()
    {
        int collidingLayers = Physics2D.GetLayerCollisionMask(gameObject.layer);
        movementCastFilter = new ContactFilter2D
        {
            useTriggers = false,
            useLayerMask = true,
            layerMask = collidingLayers,
        };
    }

    void UpdateMovementCastNormals(Vector2 moveDirection)
    {
        movementCastConstraints.Clear();

        if (!movementCollider) return;
        if (moveDirection.sqrMagnitude <= Mathf.Epsilon) return;

        Vector2 castDirection = moveDirection.normalized;
        float projectedSpeed = Mathf.Max(rb.linearVelocity.magnitude, currentMaxSpeed);
        float castDistance = projectedSpeed * Time.fixedDeltaTime + movementCastSkin;
        int hitCount = movementCollider.Cast(castDirection, movementCastFilter, movementCastHits, castDistance);

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D castHit = movementCastHits[i];

            if (!castHit.collider) continue;
            if (castHit.collider.CompareTag("Puk")) continue;
            if (castHit.normal.sqrMagnitude <= Mathf.Epsilon) continue;

            float availableDistance = Mathf.Max(0f, castHit.distance - movementCastSkin);
            float maximumInwardSpeed = availableDistance / Time.fixedDeltaTime;
            movementCastConstraints.Add(new MovementCastConstraint(castHit.normal, maximumInwardSpeed));
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateCollisionNormal(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        UpdateCollisionNormal(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (!solidCollisionNormals.ContainsKey(collision.collider)) return;

        collisionNormalExpiryTimes[collision.collider] = Time.fixedTime + collisionNormalRetentionTime;
    }

    void UpdateCollisionNormal(Collision2D collision)
    {
        if (collision.collider.CompareTag("Puk")) return;
        if (collision.contactCount == 0) return;

        Vector2 combinedNormal = Vector2.zero;

        for (int i = 0; i < collision.contactCount; i++)
        {
            combinedNormal += collision.GetContact(i).normal;
        }

        if (combinedNormal.sqrMagnitude <= Mathf.Epsilon) return;

        solidCollisionNormals[collision.collider] = combinedNormal.normalized;
        collisionNormalExpiryTimes[collision.collider] = float.PositiveInfinity;
    }

    void RemoveExpiredCollisionNormals()
    {
        expiredCollisionNormals.Clear();

        foreach (KeyValuePair<Collider2D, float> collisionNormalExpiry in collisionNormalExpiryTimes)
        {
            bool colliderWasDestroyed = !collisionNormalExpiry.Key;
            bool retentionExpired = Time.fixedTime >= collisionNormalExpiry.Value;

            if (colliderWasDestroyed || retentionExpired)
            {
                expiredCollisionNormals.Add(collisionNormalExpiry.Key);
            }
        }

        foreach (Collider2D expiredCollider in expiredCollisionNormals)
        {
            solidCollisionNormals.Remove(expiredCollider);
            collisionNormalExpiryTimes.Remove(expiredCollider);
        }
    }

    Vector2 GetMousePosition()
    {
        var distortedMouseDelta = InputManager.Instance.GetDistortedMouseDelta();
        var mousePos = GetCam().ScreenToWorldPoint(distortedMouseDelta) - GetCam().transform.position;

        return mousePos;
    }

    Vector2 GetVirtualMousePosition()
    {
        return GetMousePosition() + virtualMouseOffset;
    }

    void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude <= currentMaxSpeed) return;

        rb.linearVelocity = rb.linearVelocity.normalized * currentMaxSpeed;
    }

    void OnDashStarted()
    {
        isReturningFromDash = true;
    }

    void UpdateDashReturnTarget()
    {
        bool reachedDashReturnPosition = Vector2.Distance(transform.position, GetVirtualMousePosition()) <= stoppingDistance;
        if (reachedDashReturnPosition)
        {
            isReturningFromDash = false;
        }
    }

    void DisableInput(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        inputDisabled = !inputDisabled;
    }

    /// <summary> Subtracted given vector with transform.pos </summary>
    Vector2 GetRawDirection(Vector3 vecToCompare)
    {
        return vecToCompare - transform.position;
    }
}
