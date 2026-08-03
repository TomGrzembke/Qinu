using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary> NPC movement with ArenaMode states  
/// Disclaimer: I used Codex for the complex Math calculations of this script.</summary>
public class NPCNav : NavCalc
{
    /// <summary> The number of points looked at when the approaching the ball</summary>
    const int APPROACH_CANDIDATE_SAMPLE_COUNT = 7;

    /// <summary>How often the approaches are resampled to find the best attempt</summary>
    const int APPROACH_DISTANCE_ATTEMPTS = 4;
    const float APPROACH_REPLAN_INTERVALL = 0.1f;
    const float ROUTE_REACHED_CLEARANCE_FACTOR = 0.2f;
    const float NAVMESH_SAMPLE_RADIUS_FACTOR = 0.5f;
    const float MIN_NAVMESH_SAMPLE_RADIUS = 0.25f;
    const float HALF_ROTATION_DEGREES = 180f;
    const float APPROACH_DISTANCE_PENALTY_WEIGHT = 0.5f;
    const float BLOCKED_PUK_PATH_SCORE_PENALTY = 1000f;

    public enum ArenaMode
    {
        ToArena = 0,
        Arena = 10,
        Despawn = 20,
    }

    enum PukIntent
    {
        AttackGoal = 0,
        ClearUp = 10,
        ClearDown = 20,
        EmergencyBlock = 30,
    }

    enum EmergencyPhase
    {
        None = -1,
        RouteBehind = 10,
        Backdash = 20,
        Clear = 30,
    }

    [SerializeField] ArenaMode arenaMode;

    [field: SerializeField] public bool IsRight { get; private set; }
    [SerializeField] float arenaTransitionDistance = 2;

    [SerializeField] MoveRB moveRB;
    [SerializeField] Collider2D characterCollider;

    [SerializeField] Vector3 targetPos;
    [SerializeField] Transform defaultTransform;

    Vector2 predictedPukPosition;
    Vector2 predictedGoalCrossing;
    Vector2 defensiveInterceptPosition;
    Vector2 emergencyRoutePosition;
    Vector2 emergencyRoutePukPosition;
    Vector2 shotDirection;
    Vector2 approachPosition;
    float shotAlignment;
    bool canSafelyStrike;
    bool directlyBlockingThreat;
    bool hasShotPlan;
    bool hasApproachPlan;
    PukIntent currentPukIntent;
    PukIntent previousPukIntent;
    EmergencyPhase emergencyPhase;
    float ownGoalThreatAlignment;
    float ownGoalDistance;
    float timeToGoalLine;
    bool hasPredictedGoalCrossing;
    bool usesVerticalEmergencyFallback;
    float nextApproachPlanTime;
    readonly List<Vector2> validApproachCandidates = new();
    readonly List<Vector2> rejectedApproachCandidates = new();
    NavMeshPath candidatePath;
    Collider2D arenaCollider;

    Collider2D PukCollider => MinigameManager.Instance.PukCollider;
    Transform Puk => MinigameManager.Instance.Puk;
    Transform ArenaMiddle => MinigameManager.Instance.ArenaMiddle;
    NPCCharSO CharSO => (NPCCharSO)sOHolder.CharSO;
    CharNPCSettings NPCSettings => CharSO.CharSettings.CharNPCSettings;
    NPCRigidSettings RigidSettings => CharSO.CharSettings.CharRigidSettings;

    bool PukOnSide => IsRight ? ArenaMiddle.position.x < Puk.position.x : ArenaMiddle.position.x > Puk.position.x;


    void Start()
    {
        candidatePath = new NavMeshPath();

        if (agent.isOnNavMesh)
        {
            agent.Warp(transform.position);
        }

        StartCoroutine(DefaultSwitchRoutine());
    }

    void Update()
    {
        SyncAgentPosition();

        switch (arenaMode)
        {
            case ArenaMode.ToArena:
                hasShotPlan = false;
                UpdateToArena();
                break;
            case ArenaMode.Arena:
                UpdateInArena();
                break;
            case ArenaMode.Despawn:
                hasShotPlan = false;
                UpdateDespawn();
                break;
        }

        SetAgentPosition(targetPos);
    }

    void SyncAgentPosition()
    {
        if (!agent.isOnNavMesh) return;

        agent.nextPosition = transform.position;
    }

    void UpdateToArena()
    {
        if (defaultTransform)
        {
            targetPos = defaultTransform.position;
        }

        if (Vector3.Distance(targetPos, transform.position) < arenaTransitionDistance)
        {
            SetArenaMode(ArenaMode.Arena);
        }
    }

    void UpdateInArena()
    {
        if (!MinigameManager.Instance) return;

        if (PukOnSide)
        {
            ChasePuk();
        }
        else
        {
            hasShotPlan = false;
            emergencyPhase = EmergencyPhase.None;
            Defend();
        }
    }

    void UpdateDespawn()
    {
        targetPos = DespawnPos.position;
    }

    void ChasePuk()
    {
        directlyBlockingThreat = false;

        if (!TryGetGoalPositions(out Vector2 ownGoalMiddle, out Vector2 opponentGoalMiddle))
        {
            ChasePukWithoutGoalPlan();
            return;
        }

        UpdatePredictedPukPosition(ownGoalMiddle);
        UpdatePukIntent(ownGoalMiddle);
        UpdateShotPlan(ownGoalMiddle, opponentGoalMiddle);

        if (TryBlockImmediateThreat(ownGoalMiddle)) return;

        if (currentPukIntent == PukIntent.EmergencyBlock)
        {
            UpdateEmergencyBlock();
            return;
        }

        UpdateApproachPlan();
        UpdateChaseTarget();
        TryDashAtPuk();
    }

    bool TryGetGoalPositions(out Vector2 ownGoalMiddle, out Vector2 opponentGoalMiddle)
    {
        bool foundOwnGoal = MinigameManager.Instance.TryGetGoalMiddle(IsRight, out ownGoalMiddle);
        if (!foundOwnGoal)
        {
            opponentGoalMiddle = default;
            return false;
        }

        bool foundOpponentGoal = MinigameManager.Instance.TryGetGoalMiddle(!IsRight, out opponentGoalMiddle);
        return foundOpponentGoal;
    }

    void ChasePukWithoutGoalPlan()
    {
        hasShotPlan = false;
        targetPos = Puk.position;
    }

    void UpdatePredictedPukPosition(Vector2 ownGoalMiddle)
    {
        predictedPukPosition = Puk.position.RemoveZ() + MinigameManager.Instance.PukRB.linearVelocity * NPCSettings.PukPredictionTime;
        predictedPukPosition = ClampToArenaSideOfGoalLine(predictedPukPosition, ownGoalMiddle);
    }

    void UpdatePukIntent(Vector2 ownGoalMiddle)
    {
        currentPukIntent = GetPukIntent(ownGoalMiddle);

        bool needsEmergencyPhase = currentPukIntent == PukIntent.EmergencyBlock;
        bool hasNoEmergencyPhase = emergencyPhase == EmergencyPhase.None;

        if (needsEmergencyPhase && hasNoEmergencyPhase)
        {
            emergencyPhase = EmergencyPhase.Backdash;
        }

        bool intentChanged = currentPukIntent != previousPukIntent;
        if (!intentChanged) return;

        hasApproachPlan = false;
        usesVerticalEmergencyFallback = false;
        emergencyPhase = needsEmergencyPhase ? EmergencyPhase.Backdash : EmergencyPhase.None;
        previousPukIntent = currentPukIntent;
    }

    void UpdateShotPlan(Vector2 ownGoalMiddle, Vector2 opponentGoalMiddle)
    {
        bool isEmergencyBlock = currentPukIntent == PukIntent.EmergencyBlock;

        shotDirection = GetDesiredShotDirection(currentPukIntent, ownGoalMiddle, opponentGoalMiddle);

        Vector2 alignmentPosition = isEmergencyBlock ? Puk.position.RemoveZ() : predictedPukPosition;
        Vector2 characterToPuk = (alignmentPosition - transform.position.RemoveZ()).normalized;
        shotAlignment = Vector2.Dot(characterToPuk, shotDirection);
        float requiredAlignment = isEmergencyBlock ? NPCSettings.EmergencyClearAlignment : NPCSettings.RequiredShotAlignment;
        canSafelyStrike = shotAlignment >= requiredAlignment;

        bool isAttackingGoal = currentPukIntent == PukIntent.AttackGoal;
        bool reachedApproachPosition = false;
        if (isAttackingGoal)
        {
            reachedApproachPosition = HasPhysicallyReachedApproach();
        }

        if (isAttackingGoal && reachedApproachPosition)
        {
            canSafelyStrike = true;
        }

        hasShotPlan = true;
    }

    bool TryBlockImmediateThreat(Vector2 ownGoalMiddle)
    {
        bool isDefensiveIntent = currentPukIntent != PukIntent.AttackGoal;
        if (!isDefensiveIntent) return false;

        bool isBetweenPukAndGoal = IsBetweenPukAndGoal(ownGoalMiddle);
        bool isInThreatPath = hasPredictedGoalCrossing || isBetweenPukAndGoal;
        directlyBlockingThreat = isInThreatPath;

        if (!directlyBlockingThreat) return false;

        usesVerticalEmergencyFallback = false;
        bool isEmergencyBlock = currentPukIntent == PukIntent.EmergencyBlock;
        emergencyPhase = isEmergencyBlock ? EmergencyPhase.Clear : EmergencyPhase.None;
        defensiveInterceptPosition = hasPredictedGoalCrossing ? GetDefensiveInterceptPosition() : predictedPukPosition;
        approachPosition = defensiveInterceptPosition;
        targetPos = defensiveInterceptPosition;
        hasApproachPlan = false;
        return true;
    }

    void UpdateApproachPlan()
    {
        bool needsSaferPosition = !canSafelyStrike;
        bool hasNoApproachPlan = !hasApproachPlan;
        bool planHasExpired = Time.time >= nextApproachPlanTime;
        bool shouldReplanApproach = needsSaferPosition && (hasNoApproachPlan || planHasExpired);

        if (shouldReplanApproach)
        {
            approachPosition = FindBestApproachPosition(predictedPukPosition);
            hasApproachPlan = true;
            nextApproachPlanTime = Time.time + APPROACH_REPLAN_INTERVALL;
        }

        if (canSafelyStrike)
        {
            approachPosition = predictedPukPosition - shotDirection * NPCSettings.PukApproachDistance;
            hasApproachPlan = false;
        }
    }

    void UpdateChaseTarget()
    {
        targetPos = canSafelyStrike ? Puk.position : approachPosition;
    }

    void TryDashAtPuk()
    {
        bool randomDashEnabled = NPCSettings.DashRandomly;
        bool canAttemptDash = canSafelyStrike && randomDashEnabled;
        if (!canAttemptDash) return;

        bool passedRandomDashCheck = Random.value <= NPCSettings.ProbabilityPerFrame;

        if (passedRandomDashCheck)
        {
            moveRB.DashAtPosition(Puk.position);
        }
    }

    void UpdateEmergencyBlock()
    {
        UpdateEmergencyClearPhase();

        if (TryClearPukDuringEmergency()) return;
        if (TryFollowEmergencyRoute()) return;

        if (UpdateEmergencyApproachPlan()) return;

        targetPos = approachPosition;
        TryDefensiveDash();
    }

    void UpdateEmergencyClearPhase()
    {
        bool isBackingUp = emergencyPhase == EmergencyPhase.Backdash;
        bool isReadyToClear = isBackingUp && canSafelyStrike;
        if (!isReadyToClear) return;

        emergencyPhase = EmergencyPhase.Clear;
        hasApproachPlan = false;
    }

    bool TryClearPukDuringEmergency()
    {
        bool isClearingPuk = emergencyPhase == EmergencyPhase.Clear;
        if (!isClearingPuk) return false;

        usesVerticalEmergencyFallback = false;
        approachPosition = Puk.position.RemoveZ() - shotDirection * NPCSettings.PukApproachDistance;
        targetPos = Puk.position;
        return true;
    }

    bool TryFollowEmergencyRoute()
    {
        bool isRoutingBehindPuk = emergencyPhase == EmergencyPhase.RouteBehind;
        if (!isRoutingBehindPuk) return false;

        usesVerticalEmergencyFallback = false;
        float routeReachedDistance = Mathf.Max(GetCombinedPukClearance() * ROUTE_REACHED_CLEARANCE_FACTOR, MIN_NAVMESH_SAMPLE_RADIUS);
        bool hasReachedRoute = Vector2.Distance(transform.position, emergencyRoutePosition) <= routeReachedDistance;
        bool pukStayedNearRouteStart = Vector2.Distance(Puk.position, emergencyRoutePukPosition) <= routeReachedDistance;

        if (hasReachedRoute)
        {
            emergencyPhase = EmergencyPhase.Backdash;
            hasApproachPlan = false;
            return false;
        }

        if (pukStayedNearRouteStart)
        {
            approachPosition = emergencyRoutePosition;
            targetPos = emergencyRoutePosition;
            return true;
        }

        emergencyPhase = EmergencyPhase.Backdash;
        hasApproachPlan = false;
        return false;
    }

    bool UpdateEmergencyApproachPlan()
    {
        bool hasNoApproachPlan = !hasApproachPlan;
        bool planHasExpired = Time.time >= nextApproachPlanTime;
        bool shouldReplanApproach = hasNoApproachPlan || planHasExpired;
        if (!shouldReplanApproach) return false;

        approachPosition = FindBestApproachPosition(Puk.position.RemoveZ(), true);
        hasApproachPlan = true;
        nextApproachPlanTime = Time.time + APPROACH_REPLAN_INTERVALL;

        bool hasClearApproachPath = IsPathClearOfPuk(approachPosition);
        if (hasClearApproachPath)
        {
            usesVerticalEmergencyFallback = false;
            return false;
        }

        bool foundRouteBehindPuk = TryFindEmergencyRoutePosition(approachPosition, out emergencyRoutePosition);
        if (foundRouteBehindPuk)
        {
            emergencyRoutePukPosition = Puk.position;
            emergencyPhase = EmergencyPhase.RouteBehind;
            targetPos = emergencyRoutePosition;
            return true;
        }

        SetVerticalEmergencyFallback();
        return true;
    }

    void TryDefensiveDash()
    {
        bool defensiveDashEnabled = NPCSettings.DashDefensively;
        if (!defensiveDashEnabled) return;

        bool hasClearDashPath = HasClearDashPath(approachPosition);

        if (hasClearDashPath)
        {
            moveRB.DashAtPosition(approachPosition);
        }
    }

    void SetVerticalEmergencyFallback()
    {
        usesVerticalEmergencyFallback = true;
        shotDirection = GetClearDirection();
        approachPosition = Puk.position.RemoveZ() + shotDirection * GetCombinedPukClearance();
        targetPos = approachPosition;
    }

    bool HasPhysicallyReachedApproach()
    {
        if (!hasApproachPlan) return false;

        float approachReach = Mathf.Max(GetColliderRadius(characterCollider), agent.radius);
        return Vector2.Distance(transform.position, approachPosition) <= approachReach;
    }

    PukIntent GetPukIntent(Vector2 ownGoalMiddle)
    {
        Vector2 puckVelocity = MinigameManager.Instance.PukRB.linearVelocity;
        Vector2 puckToOwnGoal = ownGoalMiddle - Puk.position.RemoveZ();
        ownGoalDistance = puckToOwnGoal.magnitude;
        ownGoalThreatAlignment = puckVelocity.sqrMagnitude > 0f ? Vector2.Dot(puckVelocity.normalized, puckToOwnGoal.normalized) : -1f;
        hasPredictedGoalCrossing = TryGetPredictedGoalCrossing(ownGoalMiddle, out predictedGoalCrossing, out timeToGoalLine);

        if (ownGoalDistance <= NPCSettings.EmergencyGoalDistance)
        {
            return PukIntent.EmergencyBlock;
        }

        bool isNearOwnGoal = ownGoalDistance <= NPCSettings.OwnGoalDangerDistance;
        bool isMovingFastEnough = puckVelocity.magnitude >= NPCSettings.MinimumThreatSpeed;
        bool isMovingTowardOwnGoal = ownGoalThreatAlignment >= NPCSettings.OwnGoalThreatAlignment;
        bool isHeadingIntoGoal = hasPredictedGoalCrossing || isMovingTowardOwnGoal;
        bool isThreat = isNearOwnGoal && isMovingFastEnough && isHeadingIntoGoal;

        if (!isThreat) return PukIntent.AttackGoal;

        Vector2 clearDirection = GetClearDirection();
        return clearDirection.y >= 0f ? PukIntent.ClearUp : PukIntent.ClearDown;
    }

    Vector2 GetDesiredShotDirection(PukIntent intent, Vector2 ownGoalMiddle, Vector2 opponentGoalMiddle)
    {
        return intent switch
        {
            PukIntent.ClearUp => Vector2.up,
            PukIntent.ClearDown => Vector2.down,

            PukIntent.EmergencyBlock => GetGoalArenaDirection(IsRight, ownGoalMiddle),

            _ => (opponentGoalMiddle - predictedPukPosition).normalized,
        };

    }

    Vector2 GetClearDirection()
    {
        if (!arenaCollider)
        {
            return Puk.position.y >= ArenaMiddle.position.y ? Vector2.up : Vector2.down;
        }

        float spaceAbove = arenaCollider.bounds.max.y - Puk.position.y;
        float spaceBelow = Puk.position.y - arenaCollider.bounds.min.y;
        return spaceAbove >= spaceBelow ? Vector2.up : Vector2.down;
    }

    bool IsBetweenPukAndGoal(Vector2 ownGoalMiddle)
    {
        Vector2 pukPosition = Puk.position.RemoveZ();
        Vector2 arenaDirection = GetGoalArenaDirection(IsRight, ownGoalMiddle);
        float pukArenaDistance = Vector2.Dot(pukPosition - ownGoalMiddle, arenaDirection);
        float characterArenaDistance = Vector2.Dot(transform.position.RemoveZ() - ownGoalMiddle, arenaDirection);

        bool pukIsInsideArena = pukArenaDistance > 0f;
        bool characterIsInsideArena = characterArenaDistance >= 0f;
        bool characterIsBehindPuk = characterArenaDistance < pukArenaDistance;

        bool isBetweenPukAndGoal = pukIsInsideArena && characterIsInsideArena && characterIsBehindPuk;
        return isBetweenPukAndGoal;
    }

    bool TryGetPredictedGoalCrossing(Vector2 ownGoalMiddle, out Vector2 crossingPosition, out float crossingTime)
    {
        Transform goalStart = IsRight ? MinigameManager.Instance.RightGoalStart : MinigameManager.Instance.LeftGoalStart;
        Transform goalEnd = IsRight ? MinigameManager.Instance.RightGoalEnd : MinigameManager.Instance.LeftGoalEnd;
        Vector2 goalStartPosition = goalStart.position.RemoveZ();
        Vector2 goalDifference = goalEnd.position.RemoveZ() - goalStartPosition;
        Vector2 arenaDirection = GetGoalArenaDirection(IsRight, ownGoalMiddle);
        Vector2 pukPosition = Puk.position.RemoveZ();
        Vector2 pukVelocity = MinigameManager.Instance.PukRB.linearVelocity;
        float pukArenaDistance = Vector2.Dot(pukPosition - ownGoalMiddle, arenaDirection);
        float speedTowardGoal = -Vector2.Dot(pukVelocity, arenaDirection);
        crossingPosition = pukPosition;
        crossingTime = 0f;

        if (goalDifference.sqrMagnitude <= Mathf.Epsilon) return false;
        if (pukArenaDistance <= 0f || speedTowardGoal < NPCSettings.MinimumThreatSpeed) return false;

        crossingTime = pukArenaDistance / speedTowardGoal;
        crossingPosition = pukPosition + pukVelocity * crossingTime;
        float distanceAlongGoal = Vector2.Dot(crossingPosition - goalStartPosition, goalDifference.normalized);
        bool crossingIsAfterGoalStart = distanceAlongGoal >= 0f;
        bool crossingIsBeforeGoalEnd = distanceAlongGoal <= goalDifference.magnitude;

        bool crossesGoalOpening = crossingIsAfterGoalStart && crossingIsBeforeGoalEnd;
        return crossesGoalOpening;
    }

    Vector2 GetDefensiveInterceptPosition()
    {
        const int interceptSamples = 12;

        Vector2 pukPosition = Puk.position.RemoveZ();
        Vector2 pukVelocity = MinigameManager.Instance.PukRB.linearVelocity;
        Vector2 characterPosition = transform.position.RemoveZ();

        for (int i = 1; i <= interceptSamples; i++)
        {
            float interceptTime = timeToGoalLine * i / interceptSamples;
            Vector2 interceptPosition = pukPosition + pukVelocity * interceptTime;
            float reachableDistance = RigidSettings.MaxSpeed * interceptTime + agent.radius;

            if (Vector2.Distance(characterPosition, interceptPosition) <= reachableDistance)
            {
                return interceptPosition;
            }
        }

        return predictedGoalCrossing;
    }

    Vector2 ClampToArenaSideOfGoalLine(Vector2 position, Vector2 goalMiddle)
    {
        Transform goalStart = IsRight ? MinigameManager.Instance.RightGoalStart : MinigameManager.Instance.LeftGoalStart;
        Transform goalEnd = IsRight ? MinigameManager.Instance.RightGoalEnd : MinigameManager.Instance.LeftGoalEnd;
        Vector2 goalLineDirection = (goalEnd.position - goalStart.position).RemoveZ().normalized;

        if (goalLineDirection == Vector2.zero) return position;

        Vector2 arenaDirection = new(-goalLineDirection.y, goalLineDirection.x);

        if (Vector2.Dot(arenaDirection, ArenaMiddle.position.RemoveZ() - goalMiddle) < 0f)
        {
            arenaDirection = -arenaDirection;
        }

        float distanceFromGoalLine = Vector2.Dot(position - goalMiddle, arenaDirection);
        return distanceFromGoalLine < 0f ? position - arenaDirection * distanceFromGoalLine : position;
    }

    Vector2 GetGoalArenaDirection(bool isRight, Vector2 goalMiddle)
    {
        Transform goalStart = isRight ? MinigameManager.Instance.RightGoalStart : MinigameManager.Instance.LeftGoalStart;
        Transform goalEnd = isRight ? MinigameManager.Instance.RightGoalEnd : MinigameManager.Instance.LeftGoalEnd;
        Vector2 goalLineDirection = (goalEnd.position - goalStart.position).RemoveZ().normalized;

        if (goalLineDirection == Vector2.zero) return (ArenaMiddle.position.RemoveZ() - goalMiddle).normalized;

        Vector2 arenaDirection = new(-goalLineDirection.y, goalLineDirection.x);

        return Vector2.Dot(arenaDirection, ArenaMiddle.position.RemoveZ() - goalMiddle) >= 0f ? arenaDirection : -arenaDirection;
    }

    Vector2 FindBestApproachPosition(Vector2 plannedPukPosition, bool preferClearDashPath = false)
    {
        validApproachCandidates.Clear();
        rejectedApproachCandidates.Clear();

        Vector2 idealApproachDirection = -shotDirection;
        float desiredApproachDistance = currentPukIntent == PukIntent.EmergencyBlock ? Mathf.Max(NPCSettings.PukApproachDistance, GetCombinedPukClearance()) : NPCSettings.PukApproachDistance;
        float approachAlignment = currentPukIntent == PukIntent.EmergencyBlock ? NPCSettings.EmergencyClearAlignment : NPCSettings.RequiredShotAlignment;
        float maximumAngle = Mathf.Acos(Mathf.Clamp(approachAlignment, -1f, 1f)) * Mathf.Rad2Deg;
        float bestScore = float.PositiveInfinity;
        Vector2 bestPosition = currentPukIntent == PukIntent.EmergencyBlock ? plannedPukPosition + idealApproachDirection * desiredApproachDistance : plannedPukPosition;

        for (int i = 0; i < APPROACH_CANDIDATE_SAMPLE_COUNT; i++)
        {
            float angleAlpha = APPROACH_CANDIDATE_SAMPLE_COUNT == 1 ? 0f : i / (APPROACH_CANDIDATE_SAMPLE_COUNT - 1f);
            float angle = Mathf.Lerp(-maximumAngle, maximumAngle, angleAlpha);
            Vector2 candidateDirection = Quaternion.Euler(0f, 0f, angle) * idealApproachDirection;

            for (int distanceAttempt = 0; distanceAttempt < APPROACH_DISTANCE_ATTEMPTS; distanceAttempt++)
            {
                float distanceAlpha = 1f - distanceAttempt / (float)APPROACH_DISTANCE_ATTEMPTS;
                float candidateDistance = currentPukIntent == PukIntent.EmergencyBlock ? Mathf.Max(desiredApproachDistance * distanceAlpha, GetCombinedPukClearance()) : desiredApproachDistance * distanceAlpha;
                Vector2 rawCandidate = plannedPukPosition + candidateDirection * candidateDistance;

                if (!TryEvaluateApproachCandidate(rawCandidate, plannedPukPosition, angle, preferClearDashPath, out Vector2 sampledCandidate, out float score))
                {
                    rejectedApproachCandidates.Add(rawCandidate);
                    continue;
                }

                validApproachCandidates.Add(sampledCandidate);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestPosition = sampledCandidate;
                }

                break;
            }
        }

        return bestPosition;
    }

    bool TryEvaluateApproachCandidate(Vector2 rawCandidate, Vector2 plannedPukPosition, float angle, bool preferClearDashPath, out Vector2 sampledCandidate, out float score)
    {
        sampledCandidate = rawCandidate;
        score = float.PositiveInfinity;

        if (arenaCollider && !arenaCollider.OverlapPoint(rawCandidate)) return false;

        float sampleRadius = Mathf.Max(agent.radius * NAVMESH_SAMPLE_RADIUS_FACTOR, MIN_NAVMESH_SAMPLE_RADIUS);

        if (!NavMesh.SamplePosition(rawCandidate, out NavMeshHit navHit, sampleRadius, agent.areaMask)) return false;

        sampledCandidate = navHit.position.RemoveZ();

        if (arenaCollider && !arenaCollider.OverlapPoint(sampledCandidate)) return false;

        float requiredAlignment = currentPukIntent == PukIntent.EmergencyBlock ? NPCSettings.EmergencyClearAlignment : NPCSettings.RequiredShotAlignment;
        Vector2 sampledShotDifference = plannedPukPosition - sampledCandidate;

        if (sampledShotDifference.sqrMagnitude <= Mathf.Epsilon) return false;
        if (Vector2.Dot(sampledShotDifference.normalized, shotDirection) < requiredAlignment) return false;
        if (!agent.CalculatePath(navHit.position, candidatePath)) return false;
        if (candidatePath.status != NavMeshPathStatus.PathComplete) return false;

        float pathLength = GetPathLength(candidatePath);
        float anglePenalty = Mathf.Abs(angle) / HALF_ROTATION_DEGREES * NPCSettings.PukApproachDistance;
        float distancePenalty = Mathf.Abs(NPCSettings.PukApproachDistance - Vector2.Distance(plannedPukPosition, sampledCandidate)) * APPROACH_DISTANCE_PENALTY_WEIGHT;
        score = pathLength + anglePenalty + distancePenalty;

        bool hasClearPukPath = true;
        if (preferClearDashPath)
        {
            hasClearPukPath = IsCalculatedPathClearOfPuk(candidatePath);
        }

        bool shouldPenalizeBlockedPath = preferClearDashPath && !hasClearPukPath;

        if (shouldPenalizeBlockedPath)
        {
            score += BLOCKED_PUK_PATH_SCORE_PENALTY;
        }

        return true;
    }

    bool HasClearDashPath(Vector2 dashTarget)
    {
        return IsSegmentClearOfPuk(transform.position.RemoveZ(), dashTarget);
    }

    bool IsPathClearOfPuk(Vector2 targetPosition)
    {
        if (!agent.CalculatePath(targetPosition, candidatePath)) return false;
        if (candidatePath.status != NavMeshPathStatus.PathComplete) return false;

        Vector2 previousPosition = transform.position.RemoveZ();

        foreach (Vector3 corner in candidatePath.corners)
        {
            Vector2 cornerPosition = corner.RemoveZ();

            if (!IsSegmentClearOfPuk(previousPosition, cornerPosition)) return false;

            previousPosition = cornerPosition;
        }

        return true;
    }

    bool TryFindEmergencyRoutePosition(Vector2 behindPukPosition, out Vector2 routePosition)
    {
        Vector2 pukPosition = Puk.position.RemoveZ();
        Vector2 behindDifference = behindPukPosition - pukPosition;
        Vector2 behindDirection = behindDifference.sqrMagnitude > Mathf.Epsilon ? behindDifference.normalized : -shotDirection;
        Vector2 routeDirection = new(-behindDirection.y, behindDirection.x);
        float routeOffset = GetCombinedPukClearance() * NPCSettings.EmergencyRouteWidth;
        float bestPathLength = float.PositiveInfinity;
        routePosition = behindPukPosition;

        for (int directionSign = -1; directionSign <= 1; directionSign += 2)
        {
            Vector2 rawRoutePosition = behindPukPosition + routeDirection * routeOffset * directionSign;

            if (arenaCollider && !arenaCollider.OverlapPoint(rawRoutePosition)) continue;
            if (!NavMesh.SamplePosition(rawRoutePosition, out NavMeshHit navHit, Mathf.Max(agent.radius, MIN_NAVMESH_SAMPLE_RADIUS), agent.areaMask)) continue;

            Vector2 sampledRoutePosition = navHit.position.RemoveZ();

            if (arenaCollider && !arenaCollider.OverlapPoint(sampledRoutePosition)) continue;
            if (!agent.CalculatePath(navHit.position, candidatePath)) continue;
            if (candidatePath.status != NavMeshPathStatus.PathComplete) continue;
            if (!IsCalculatedPathClearOfPuk(candidatePath)) continue;
            if (!IsSegmentClearOfPuk(sampledRoutePosition, behindPukPosition)) continue;

            float pathLength = GetPathLength(candidatePath);

            if (pathLength >= bestPathLength) continue;

            bestPathLength = pathLength;
            routePosition = sampledRoutePosition;
        }

        return bestPathLength < float.PositiveInfinity;
    }

    bool IsCalculatedPathClearOfPuk(NavMeshPath path)
    {
        Vector2 previousPosition = transform.position.RemoveZ();

        foreach (Vector3 corner in path.corners)
        {
            Vector2 cornerPosition = corner.RemoveZ();

            if (!IsSegmentClearOfPuk(previousPosition, cornerPosition)) return false;

            previousPosition = cornerPosition;
        }

        return true;
    }

    bool IsSegmentClearOfPuk(Vector2 segmentStart, Vector2 segmentEnd)
    {
        Vector2 pukPosition = Puk.position.RemoveZ();
        Vector2 segmentDifference = segmentEnd - segmentStart;

        if (segmentDifference.sqrMagnitude <= Mathf.Epsilon)
        {
            bool isPointClearOfPuk = Vector2.Distance(segmentStart, pukPosition) >= GetCombinedPukClearance();
            return isPointClearOfPuk;
        }

        float segmentAlpha = Mathf.Clamp01(Vector2.Dot(pukPosition - segmentStart, segmentDifference) / segmentDifference.sqrMagnitude);
        Vector2 closestPoint = segmentStart + segmentDifference * segmentAlpha;
        bool isSegmentClearOfPuk = Vector2.Distance(closestPoint, pukPosition) >= GetCombinedPukClearance();

        return isSegmentClearOfPuk;
    }

    float GetCombinedPukClearance()
    {
        return GetColliderRadius(characterCollider) + GetColliderRadius(PukCollider) + NPCSettings.EmergencyDashPukClearance;
    }

    float GetColliderRadius(Collider2D targetCollider)
    {
        if (!targetCollider) return 0f;

        return Mathf.Max(targetCollider.bounds.extents.x, targetCollider.bounds.extents.y);
    }

    float GetPathLength(NavMeshPath path)
    {
        float length = 0f;

        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        return length;
    }

    void Defend()
    {
        if (NPCSettings.FollowBallY == false)
        {
            if (NPCSettings.GoesToDefault)
            {
                targetPos = defaultTransform.position;
            }

            return;
        }

        targetPos.x = defaultTransform.position.x;
        targetPos.y = NPCSettings.InvertY ? -Puk.position.y : Puk.position.y;
    }

    protected override float GetStoppingDistance()
    {
        bool isInArena = arenaMode == ArenaMode.Arena;
        bool hasMinigameManager = MinigameManager.Instance;
        bool isChasingPuk = isInArena && hasMinigameManager && PukOnSide;

        if (isChasingPuk) return 0f;

        return CharSO.StoppingDistance;
    }

    public void SideSettings(bool _isRight)
    {
        IsRight = _isRight;
    }

    public void GoHome()
    {
        arenaMode = ArenaMode.Despawn;
        SetAgentPosition(DespawnPos);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Arena")) return;

        arenaCollider = collision;
        SetArenaMode(ArenaMode.Arena);
    }

    public void SetArenaMode(ArenaMode newMode)
    {
        arenaMode = newMode;
    }

    public void ToArena()
    {
        if (arenaMode != ArenaMode.Arena)
        {
            arenaMode = ArenaMode.ToArena;
        }

        defaultTransform = GetRandomDefaultTransform();
    }

    /// <summary>Switches the default position of the character</summary>
    IEnumerator DefaultSwitchRoutine()
    {
        while (NPCSettings.GoesToDefault)
        {
            var waitTime = NPCSettings.DefaultSwitchTime;

            if (waitTime <= 0) yield break;

            yield return new WaitForSeconds(waitTime);

            defaultTransform = GetRandomDefaultTransform();
        }
    }

    Transform GetRandomDefaultTransform()
    {
        return TournamentManager.Instance.GetRandomDefaultTrans(IsRight ? 1 : 0);
    }

    void OnDrawGizmosSelected()
    {
        if (!MinigameManager.Instance) return;
        if (!MinigameManager.Instance.TryGetGoalMiddle(IsRight, out Vector2 ownGoalMiddle)) return;

        Transform goalStart = IsRight ? MinigameManager.Instance.RightGoalStart : MinigameManager.Instance.LeftGoalStart;
        Transform goalEnd = IsRight ? MinigameManager.Instance.RightGoalEnd : MinigameManager.Instance.LeftGoalEnd;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(goalStart.position, goalEnd.position);
        Gizmos.DrawWireSphere(ownGoalMiddle, 0.3f);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(ownGoalMiddle, NPCSettings.OwnGoalDangerDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ownGoalMiddle, NPCSettings.EmergencyGoalDistance);

        if (!hasShotPlan) return;

        Vector3 characterPosition = transform.position;
        Vector3 pukPosition = Puk.position;
        Vector3 predictedPosition = predictedPukPosition;
        Vector3 safeApproachPosition = approachPosition;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(pukPosition, 0.3f);
        Gizmos.DrawLine(pukPosition, predictedPosition);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(predictedPosition, 0.3f);

        if (hasPredictedGoalCrossing)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(predictedGoalCrossing, 0.45f);
            Gizmos.DrawLine(pukPosition, predictedGoalCrossing);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(defensiveInterceptPosition, 0.35f);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(safeApproachPosition, 0.4f);
        Gizmos.DrawLine(safeApproachPosition, predictedPosition);

        Gizmos.color = Color.white;

        foreach (Vector2 candidate in validApproachCandidates)
        {
            Gizmos.DrawWireSphere(candidate, 0.2f);
        }

        Gizmos.color = new Color(1f, 0.3f, 0.3f);

        foreach (Vector2 candidate in rejectedApproachCandidates)
        {
            Gizmos.DrawWireCube(candidate, Vector3.one * 0.25f);
        }

        Gizmos.color = GetIntentColor();
        DrawArrow(predictedPosition, shotDirection, 4f);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawLine(pukPosition, ownGoalMiddle);

        Gizmos.color = canSafelyStrike ? Color.green : Color.red;
        Gizmos.DrawLine(characterPosition, predictedPosition);
        Gizmos.DrawWireSphere(targetPos, 0.5f);
        Gizmos.DrawLine(characterPosition, targetPos);

        if (emergencyPhase == EmergencyPhase.RouteBehind)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(emergencyRoutePosition, 0.45f);
            Gizmos.DrawLine(characterPosition, emergencyRoutePosition);
        }

#if UNITY_EDITOR
        float requiredAlignment = currentPukIntent == PukIntent.EmergencyBlock ? NPCSettings.EmergencyClearAlignment : NPCSettings.RequiredShotAlignment;
        string movementStatus = directlyBlockingThreat ? "DIRECTLY BLOCKING" : canSafelyStrike ? "SAFE TO CLEAR" : "REPOSITIONING";

        if (usesVerticalEmergencyFallback)
        {
            movementStatus = "VERTICAL LAST-CHANCE CLEAR";
        }

        Handles.Label(pukPosition + Vector3.up * 0.5f, "Puck");
        Handles.Label(predictedPosition + Vector3.up * 0.5f, "Predicted puck");
        Handles.Label(safeApproachPosition + Vector3.up * 0.5f, emergencyPhase == EmergencyPhase.Backdash ? "Backdash target" : "Safe approach");
        Handles.Label(characterPosition + Vector3.up, $"Intent: {currentPukIntent}\nEmergency: {emergencyPhase}\nShot: {shotAlignment:F2} / {requiredAlignment:F2}\nThreat: {ownGoalThreatAlignment:F2} / {NPCSettings.OwnGoalThreatAlignment:F2}\nGoal distance: {ownGoalDistance:F1}\nGoal crossing: {(hasPredictedGoalCrossing ? $"{timeToGoalLine:F2}s" : "None")}\n{movementStatus}");
#endif
    }

    void DrawArrow(Vector3 start, Vector2 direction, float length)
    {
        Vector3 end = start + (Vector3)(direction * length);
        Vector2 perpendicular = new(-direction.y, direction.x);
        Vector3 arrowBase = end - (Vector3)(direction * 0.6f);

        Gizmos.DrawLine(start, end);
        Gizmos.DrawLine(end, arrowBase + (Vector3)(perpendicular * 0.3f));
        Gizmos.DrawLine(end, arrowBase - (Vector3)(perpendicular * 0.3f));
    }

    Color GetIntentColor()
    {
        return currentPukIntent switch
        {
            PukIntent.ClearUp or PukIntent.ClearDown => Color.cyan,
            PukIntent.EmergencyBlock => new Color(1f, 0.5f, 0f),
            _ => Color.green,
        };

    }
}
