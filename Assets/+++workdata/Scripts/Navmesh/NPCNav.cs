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
    const int STUCK_RECOVERY_DIRECTION_COUNT = 8;
    const float STUCK_RECOVERY_DIRECTION_STEP = 45f;
    const float DESPAWN_ARRIVAL_TOLERANCE = 0.05f;
    const float ALIGNMENT_COMPARISON_TOLERANCE = 0.001f;

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

    sealed class ShotPlan
    {
        public Vector2 PredictedPukPosition;
        public Vector2 PredictedGoalCrossing;
        public Vector2 DefensiveInterceptPosition;
        public Vector2 Direction;
        public float Alignment;
        public float OwnGoalThreatAlignment;
        public float OwnGoalDistance;
        public float TimeToGoalLine;
        public bool CanSafelyStrike;
        public bool DirectlyBlockingThreat;
        public bool HasPlan;
        public bool HasPredictedGoalCrossing;
        public PukIntent CurrentIntent;
        public PukIntent PreviousIntent;
    }

    sealed class EmergencyPlan
    {
        public Vector2 RoutePosition;
        public Vector2 RoutePukPosition;
        public EmergencyPhase Phase;
        public bool UsesVerticalFallback;
    }

    sealed class ApproachPlan
    {
        public Vector2 Position;
        public Vector2 RoutePosition;
        public float NextPlanTime;
        public bool HasPlan;
        public bool HasBeenReached;
        public bool IsRoutingAroundPuk;
        public NavMeshPath CandidatePath;
    }

    sealed class ApproachDebug
    {
        public readonly List<Vector2> ValidCandidates = new();
        public readonly List<Vector2> RejectedCandidates = new();
    }

    sealed class MovementProgress
    {
        public Vector2 SamplePosition;
        public Vector2 FailedTarget;
        public Vector2 RecoveryPosition;
        public Vector2 BlockedDirection;
        public float SampleStartTime;
        public float RecoveryEndTime;
        public int DirectionAttempt;
        public bool HasSample;
        public bool IsRecovering;
    }

    sealed class SideReturnPlan
    {
        public Vector2 RoutePosition;
        public bool IsReturning;
        public bool IsRoutingAroundPuk;
    }

    sealed class RigidbodyColliderState
    {
        public Collider2D Collider;
        public bool WasEnabled;
    }

    readonly ShotPlan shotPlan = new();
    readonly EmergencyPlan emergencyPlan = new();
    readonly ApproachPlan approachPlan = new();
    readonly ApproachDebug approachDebug = new();
    readonly MovementProgress movementProgress = new();
    readonly SideReturnPlan sideReturnPlan = new();
    readonly List<RigidbodyColliderState> rigidbodyColliderStates = new();
    Collider2D arenaCollider;
    NPCCharSO charSO;
    CharNPCSettings npcSettings;
    NPCRigidSettings rigidSettings;
    Coroutine defaultSwitchRoutine;

    Collider2D PukCollider => MinigameManager.Instance.PukCollider;
    Transform Puk => MinigameManager.Instance.Puk;
    Transform ArenaMiddle => MinigameManager.Instance.ArenaMiddle;
    CharNPCSettings NPCSettings => npcSettings;
    NPCRigidSettings RigidSettings => rigidSettings;

    bool PukOnSide => IsRight ? ArenaMiddle.position.x < Puk.position.x : ArenaMiddle.position.x > Puk.position.x;
    
    [SerializeField] ArenaMode arenaMode;

    [field: SerializeField] public bool IsRight { get; private set; }
    [SerializeField] float arenaTransitionDistance = 2;

    DashController dashController;
    [SerializeField] Collider2D characterCollider;

    [SerializeField] Vector3 targetPos;
    [SerializeField] Transform defaultTransform;


    void Start()
    {
        CacheCharSettings(sOHolder.CharSO);
        sOHolder.CharSOChanged += OnCharSOChanged;
        approachPlan.CandidatePath = new();
        dashController = GetComponent<DashController>();
        CacheRigidbodyColliders();
        SetRigidbodyCollidersEnabled(arenaMode != ArenaMode.Despawn);
        
        if (agent.isOnNavMesh)
        {
            agent.Warp(transform.position);
        }

        defaultSwitchRoutine = StartCoroutine(DefaultSwitchRoutine());
    }

    void OnDestroy()
    {
        if (sOHolder)
        {
            sOHolder.CharSOChanged -= OnCharSOChanged;
        }
    }

    void OnCharSOChanged(CharSO newCharSO)
    {
        CacheCharSettings(newCharSO);
        ResetPlansForSettingsChange();
        RestartDefaultSwitchRoutine();
    }

    void CacheCharSettings(CharSO newCharSO)
    {
        if (newCharSO is not NPCCharSO newNPCCharSO)
        {
            Debug.LogError($"{name} requires NPCCharSO settings.", this);
            return;
        }

        charSO = newNPCCharSO;
        npcSettings = charSO.CharSettings.CharNPCSettings;
        rigidSettings = charSO.CharSettings.CharRigidSettings;
    }

    void ResetPlansForSettingsChange()
    {
        shotPlan.HasPlan = false;
        shotPlan.CanSafelyStrike = false;
        shotPlan.DirectlyBlockingThreat = false;
        approachPlan.HasPlan = false;
        approachPlan.HasBeenReached = false;
        approachPlan.IsRoutingAroundPuk = false;
        approachPlan.NextPlanTime = 0f;
        emergencyPlan.Phase = EmergencyPhase.None;
        emergencyPlan.UsesVerticalFallback = false;
        approachDebug.ValidCandidates.Clear();
        approachDebug.RejectedCandidates.Clear();
        movementProgress.HasSample = false;
        movementProgress.IsRecovering = false;
        movementProgress.BlockedDirection = Vector2.zero;
        movementProgress.DirectionAttempt = 0;
        sideReturnPlan.IsReturning = false;
        sideReturnPlan.IsRoutingAroundPuk = false;
    }

    void RestartDefaultSwitchRoutine()
    {
        if (defaultSwitchRoutine != null)
        {
            StopCoroutine(defaultSwitchRoutine);
        }

        defaultSwitchRoutine = StartCoroutine(DefaultSwitchRoutine());
    }

    void Update()
    {
        SyncAgentPosition();

        switch (arenaMode)
        {
            case ArenaMode.ToArena:
                shotPlan.HasPlan = false;
                UpdateToArena();
                break;
            case ArenaMode.Arena:
                UpdateInArena();
                break;
            case ArenaMode.Despawn:
                shotPlan.HasPlan = false;
                if (UpdateDespawn()) return;
                break;
        }

        SetAgentPosition(targetPos);
        UpdateMovementProgress();
    }

    void SyncAgentPosition()
    {
        if (!agent.isOnNavMesh) return;

        agent.nextPosition = transform.position;
    }

    void UpdateToArena()
    {
        if (TryFollowStuckRecovery()) return;

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
        if (TryFollowStuckRecovery()) return;
        if (TryReturnToOwnSide()) return;

        if (PukOnSide)
        {
            ChasePuk();
        }
        else
        {
            shotPlan.HasPlan = false;
            emergencyPlan.Phase = EmergencyPhase.None;
            Defend();
        }
    }

    bool TryReturnToOwnSide()
    {
        if (!MinigameManager.Instance.TryGetGoalMiddle(IsRight, out Vector2 ownGoalMiddle)) return false;

        Vector2 arenaMiddle = ArenaMiddle.position.RemoveZ();
        Vector2 directionToOwnGoal = ownGoalMiddle - arenaMiddle;
        if (directionToOwnGoal.sqrMagnitude <= Mathf.Epsilon) return false;

        float distanceIntoOwnHalf = Vector2.Dot(transform.position.RemoveZ() - arenaMiddle, directionToOwnGoal.normalized);
        float returnCompletionDistance = Mathf.Max(agent.radius, MIN_NAVMESH_SAMPLE_RADIUS);

        if (!sideReturnPlan.IsReturning && distanceIntoOwnHalf >= 0f) return false;

        if (sideReturnPlan.IsReturning && distanceIntoOwnHalf >= returnCompletionDistance)
        {
            sideReturnPlan.IsReturning = false;
            sideReturnPlan.IsRoutingAroundPuk = false;
            ResetMovementSample();
            return false;
        }

        sideReturnPlan.IsReturning = true;
        shotPlan.HasPlan = false;
        shotPlan.CanSafelyStrike = false;
        shotPlan.DirectlyBlockingThreat = false;
        emergencyPlan.Phase = EmergencyPhase.None;
        approachPlan.HasPlan = false;
        approachPlan.IsRoutingAroundPuk = false;

        Vector2 returnPosition = defaultTransform
            ? defaultTransform.position.RemoveZ()
            : arenaMiddle + directionToOwnGoal.normalized * returnCompletionDistance;

        if (TryFollowSideReturnRoute()) return true;

        if (IsPathClearOfPuk(returnPosition))
        {
            targetPos = returnPosition;
            return true;
        }

        if (TryFindPukAvoidanceRoutePosition(returnPosition, out Vector2 routePosition))
        {
            sideReturnPlan.RoutePosition = routePosition;
            sideReturnPlan.IsRoutingAroundPuk = true;
            targetPos = routePosition;
            return true;
        }

        targetPos = transform.position;
        return true;
    }

    bool TryFollowSideReturnRoute()
    {
        if (!sideReturnPlan.IsRoutingAroundPuk) return false;

        float routeReachedDistance = Mathf.Max(GetCombinedPukClearance() * ROUTE_REACHED_CLEARANCE_FACTOR, MIN_NAVMESH_SAMPLE_RADIUS);
        bool reachedRoutePosition = Vector2.Distance(transform.position, sideReturnPlan.RoutePosition) <= routeReachedDistance;

        if (reachedRoutePosition)
        {
            sideReturnPlan.IsRoutingAroundPuk = false;
            return false;
        }

        targetPos = sideReturnPlan.RoutePosition;
        return true;
    }

    bool UpdateDespawn()
    {
        targetPos = DespawnPos.position;

        float despawnDistance = Mathf.Max(charSO.StoppingDistance + DESPAWN_ARRIVAL_TOLERANCE, MIN_NAVMESH_SAMPLE_RADIUS);
        if (Vector2.Distance(transform.position, DespawnPos.position) > despawnDistance) return false;

        CharManager.Instance.CharsSpawned.Remove(gameObject);
        Destroy(gameObject);
        return true;
    }

    void ChasePuk()
    {
        shotPlan.DirectlyBlockingThreat = false;

        if (!TryGetGoalPositions(out Vector2 ownGoalMiddle, out Vector2 opponentGoalMiddle))
        {
            ChasePukWithoutGoalPlan();
            return;
        }

        UpdatePredictedPukPosition(ownGoalMiddle);
        UpdatePukIntent(ownGoalMiddle);
        UpdateShotPlan(ownGoalMiddle, opponentGoalMiddle);

        if (TryBlockImmediateThreat(ownGoalMiddle)) return;

        if (shotPlan.CurrentIntent == PukIntent.EmergencyBlock)
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
        shotPlan.HasPlan = false;
        targetPos = Puk.position;
    }

    void UpdatePredictedPukPosition(Vector2 ownGoalMiddle)
    {
        shotPlan.PredictedPukPosition = Puk.position.RemoveZ() + MinigameManager.Instance.PukRB.linearVelocity * NPCSettings.PukPredictionTime;
        shotPlan.PredictedPukPosition = ClampToArenaSideOfGoalLine(shotPlan.PredictedPukPosition, ownGoalMiddle);
    }

    void UpdatePukIntent(Vector2 ownGoalMiddle)
    {
        shotPlan.CurrentIntent = GetPukIntent(ownGoalMiddle);

        bool needsEmergencyPhase = shotPlan.CurrentIntent == PukIntent.EmergencyBlock;
        bool hasNoEmergencyPhase = emergencyPlan.Phase == EmergencyPhase.None;

        if (needsEmergencyPhase && hasNoEmergencyPhase)
        {
            emergencyPlan.Phase = EmergencyPhase.Backdash;
        }

        bool intentChanged = shotPlan.CurrentIntent != shotPlan.PreviousIntent;
        if (!intentChanged) return;

        approachPlan.HasPlan = false;
        approachPlan.HasBeenReached = false;
        approachPlan.IsRoutingAroundPuk = false;
        shotPlan.CanSafelyStrike = false;
        emergencyPlan.UsesVerticalFallback = false;
        emergencyPlan.Phase = needsEmergencyPhase ? EmergencyPhase.Backdash : EmergencyPhase.None;
        shotPlan.PreviousIntent = shotPlan.CurrentIntent;
    }

    void UpdateShotPlan(Vector2 ownGoalMiddle, Vector2 opponentGoalMiddle)
    {
        bool isEmergencyBlock = shotPlan.CurrentIntent == PukIntent.EmergencyBlock;

        shotPlan.Direction = GetDesiredShotDirection(shotPlan.CurrentIntent, ownGoalMiddle, opponentGoalMiddle);

        Vector2 alignmentPosition = isEmergencyBlock ? Puk.position.RemoveZ() : shotPlan.PredictedPukPosition;
        Vector2 characterToPuk = (alignmentPosition - transform.position.RemoveZ()).normalized;
        shotPlan.Alignment = Vector2.Dot(characterToPuk, shotPlan.Direction);
        float alignmentThreshold = GetStrikeAlignmentThreshold(isEmergencyBlock, shotPlan.CanSafelyStrike);
        shotPlan.CanSafelyStrike = shotPlan.Alignment >= alignmentThreshold - ALIGNMENT_COMPARISON_TOLERANCE;
        shotPlan.CanSafelyStrike &= IsContactDirectionSafeForOwnGoal(ownGoalMiddle);

        if (movementProgress.IsRecovering)
        {
            shotPlan.CanSafelyStrike = false;
        }

        bool isAttackingGoal = shotPlan.CurrentIntent == PukIntent.AttackGoal;
        bool canCompleteApproach = isAttackingGoal && !approachPlan.HasBeenReached;
        if (canCompleteApproach && HasPhysicallyReachedApproach())
        {
            approachPlan.HasBeenReached = true;
            movementProgress.IsRecovering = false;
            ResetMovementSample();
        }

        shotPlan.HasPlan = true;
    }

    bool TryBlockImmediateThreat(Vector2 ownGoalMiddle)
    {
        bool isDefensiveIntent = shotPlan.CurrentIntent != PukIntent.AttackGoal;
        if (!isDefensiveIntent) return false;

        bool isBetweenPukAndGoal = IsBetweenPukAndGoal(ownGoalMiddle);
        bool hasSafeContactDirection = IsContactDirectionSafeForOwnGoal(ownGoalMiddle);
        bool isInThreatPath = (shotPlan.HasPredictedGoalCrossing || isBetweenPukAndGoal) && hasSafeContactDirection;
        shotPlan.DirectlyBlockingThreat = isInThreatPath;

        if (!shotPlan.DirectlyBlockingThreat) return false;

        emergencyPlan.UsesVerticalFallback = false;
        bool isEmergencyBlock = shotPlan.CurrentIntent == PukIntent.EmergencyBlock;
        emergencyPlan.Phase = isEmergencyBlock ? EmergencyPhase.Clear : EmergencyPhase.None;
        shotPlan.DefensiveInterceptPosition = shotPlan.HasPredictedGoalCrossing ? GetDefensiveInterceptPosition() : shotPlan.PredictedPukPosition;
        approachPlan.Position = shotPlan.DefensiveInterceptPosition;
        targetPos = shotPlan.DefensiveInterceptPosition;
        approachPlan.HasPlan = false;
        return true;
    }

    void UpdateApproachPlan()
    {
        bool needsSaferPosition = !shotPlan.CanSafelyStrike;
        bool hasNoApproachPlan = !approachPlan.HasPlan;
        bool planHasExpired = Time.time >= approachPlan.NextPlanTime;
        bool shouldReplanApproach = needsSaferPosition && (hasNoApproachPlan || planHasExpired);

        if (shouldReplanApproach)
        {
            approachPlan.Position = FindBestApproachPosition(shotPlan.PredictedPukPosition);
            approachPlan.HasPlan = true;
            approachPlan.HasBeenReached = false;
            approachPlan.NextPlanTime = Time.time + APPROACH_REPLAN_INTERVALL;
            UpdatePukAvoidanceRoute();
        }

    }

    void UpdateChaseTarget()
    {
        if (shotPlan.CanSafelyStrike)
        {
            approachPlan.IsRoutingAroundPuk = false;
            targetPos = Puk.position;
            return;
        }

        if (TryFollowPukAvoidanceRoute()) return;

        targetPos = approachPlan.Position;
    }

    void UpdatePukAvoidanceRoute()
    {
        approachPlan.IsRoutingAroundPuk = false;

        if (IsPathClearOfPuk(approachPlan.Position)) return;

        if (TryFindPukAvoidanceRoutePosition(approachPlan.Position, out Vector2 routePosition))
        {
            approachPlan.RoutePosition = routePosition;
            approachPlan.IsRoutingAroundPuk = true;
        }
    }

    bool TryFollowPukAvoidanceRoute()
    {
        if (!approachPlan.IsRoutingAroundPuk) return false;

        float routeReachedDistance = Mathf.Max(GetCombinedPukClearance() * ROUTE_REACHED_CLEARANCE_FACTOR, MIN_NAVMESH_SAMPLE_RADIUS);
        bool reachedRoutePosition = Vector2.Distance(transform.position, approachPlan.RoutePosition) <= routeReachedDistance;

        if (reachedRoutePosition)
        {
            approachPlan.IsRoutingAroundPuk = false;
            return false;
        }

        targetPos = approachPlan.RoutePosition;
        return true;
    }

    void TryDashAtPuk()
    {
        bool randomDashEnabled = NPCSettings.DashRandomly;
        bool canAttemptDash = shotPlan.CanSafelyStrike && randomDashEnabled;
        if (!canAttemptDash) return;

        float dashChanceThisFrame = 1f - Mathf.Exp(-NPCSettings.RandomDashesPerSecond * Time.deltaTime);
        bool passedRandomDashCheck = Random.value <= dashChanceThisFrame;

        if (passedRandomDashCheck)
        {
            dashController.DashAtTarget(Puk);
        }
    }

    void UpdateEmergencyBlock()
    {
        UpdateEmergencyClearPhase();

        if (TryClearPukDuringEmergency()) return;
        if (TryFollowEmergencyRoute()) return;

        if (UpdateEmergencyApproachPlan()) return;

        targetPos = approachPlan.Position;
        TryDefensiveDash();
    }

    void UpdateEmergencyClearPhase()
    {
        bool isBackingUp = emergencyPlan.Phase == EmergencyPhase.Backdash;
        bool isReadyToClear = isBackingUp && shotPlan.CanSafelyStrike;
        if (!isReadyToClear) return;

        emergencyPlan.Phase = EmergencyPhase.Clear;
        approachPlan.HasPlan = false;
    }

    bool TryClearPukDuringEmergency()
    {
        bool isClearingPuk = emergencyPlan.Phase == EmergencyPhase.Clear;
        if (!isClearingPuk) return false;

        emergencyPlan.UsesVerticalFallback = false;
        approachPlan.Position = Puk.position.RemoveZ() - shotPlan.Direction * NPCSettings.PukApproachDistance;
        targetPos = Puk.position;
        return true;
    }

    bool TryFollowEmergencyRoute()
    {
        bool isRoutingBehindPuk = emergencyPlan.Phase == EmergencyPhase.RouteBehind;
        if (!isRoutingBehindPuk) return false;

        emergencyPlan.UsesVerticalFallback = false;
        float routeReachedDistance = Mathf.Max(GetCombinedPukClearance() * ROUTE_REACHED_CLEARANCE_FACTOR, MIN_NAVMESH_SAMPLE_RADIUS);
        bool hasReachedRoute = Vector2.Distance(transform.position, emergencyPlan.RoutePosition) <= routeReachedDistance;
        bool pukStayedNearRouteStart = Vector2.Distance(Puk.position, emergencyPlan.RoutePukPosition) <= routeReachedDistance;

        if (hasReachedRoute)
        {
            emergencyPlan.Phase = EmergencyPhase.Backdash;
            approachPlan.HasPlan = false;
            return false;
        }

        if (pukStayedNearRouteStart)
        {
            approachPlan.Position = emergencyPlan.RoutePosition;
            targetPos = emergencyPlan.RoutePosition;
            return true;
        }

        emergencyPlan.Phase = EmergencyPhase.Backdash;
        approachPlan.HasPlan = false;
        return false;
    }

    bool UpdateEmergencyApproachPlan()
    {
        bool hasNoApproachPlan = !approachPlan.HasPlan;
        bool planHasExpired = Time.time >= approachPlan.NextPlanTime;
        bool shouldReplanApproach = hasNoApproachPlan || planHasExpired;
        if (!shouldReplanApproach) return false;

        approachPlan.Position = FindBestApproachPosition(Puk.position.RemoveZ(), true);
        approachPlan.HasPlan = true;
        approachPlan.NextPlanTime = Time.time + APPROACH_REPLAN_INTERVALL;

        bool hasClearApproachPath = IsPathClearOfPuk(approachPlan.Position);
        if (hasClearApproachPath)
        {
            emergencyPlan.UsesVerticalFallback = false;
            return false;
        }

        bool foundRouteBehindPuk = TryFindPukAvoidanceRoutePosition(approachPlan.Position, out emergencyPlan.RoutePosition);
        if (foundRouteBehindPuk)
        {
            emergencyPlan.RoutePukPosition = Puk.position;
            emergencyPlan.Phase = EmergencyPhase.RouteBehind;
            targetPos = emergencyPlan.RoutePosition;
            return true;
        }

        SetVerticalEmergencyFallback();
        return true;
    }

    void TryDefensiveDash()
    {
        bool defensiveDashEnabled = NPCSettings.DashDefensively;
        if (!defensiveDashEnabled) return;

        bool hasClearDashPath = HasClearDashPath(approachPlan.Position);

        if (hasClearDashPath)
        {
            dashController.DashAtPosition(approachPlan.Position);
        }
    }

    void SetVerticalEmergencyFallback()
    {
        emergencyPlan.UsesVerticalFallback = true;
        shotPlan.Direction = GetClearDirection();
        approachPlan.Position = Puk.position.RemoveZ() + shotPlan.Direction * GetCombinedPukClearance();
        targetPos = approachPlan.Position;
    }

    bool HasPhysicallyReachedApproach()
    {
        if (!approachPlan.HasPlan) return false;

        float approachReach = Mathf.Max(GetColliderRadius(characterCollider), agent.radius);
        return Vector2.Distance(transform.position, approachPlan.Position) <= approachReach;
    }

    PukIntent GetPukIntent(Vector2 ownGoalMiddle)
    {
        Vector2 puckVelocity = MinigameManager.Instance.PukRB.linearVelocity;
        Vector2 puckToOwnGoal = ownGoalMiddle - Puk.position.RemoveZ();
        shotPlan.OwnGoalDistance = puckToOwnGoal.magnitude;
        shotPlan.OwnGoalThreatAlignment = puckVelocity.sqrMagnitude > 0f ? Vector2.Dot(puckVelocity.normalized, puckToOwnGoal.normalized) : -1f;
        shotPlan.HasPredictedGoalCrossing = TryGetPredictedGoalCrossing(ownGoalMiddle, out shotPlan.PredictedGoalCrossing, out shotPlan.TimeToGoalLine);

        if (shotPlan.OwnGoalDistance <= NPCSettings.EmergencyGoalDistance)
        {
            return PukIntent.EmergencyBlock;
        }

        bool isNearOwnGoal = shotPlan.OwnGoalDistance <= NPCSettings.OwnGoalDangerDistance;
        bool isMovingFastEnough = puckVelocity.magnitude >= NPCSettings.MinimumThreatSpeed;
        bool isMovingTowardOwnGoal = shotPlan.OwnGoalThreatAlignment >= NPCSettings.OwnGoalThreatAlignment;
        bool isHeadingIntoGoal = shotPlan.HasPredictedGoalCrossing || isMovingTowardOwnGoal;
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

            _ => (opponentGoalMiddle - shotPlan.PredictedPukPosition).normalized,
        };

    }

    bool IsContactDirectionSafeForOwnGoal(Vector2 ownGoalMiddle)
    {
        Vector2 characterToPuk = Puk.position.RemoveZ() - transform.position.RemoveZ();
        if (characterToPuk.sqrMagnitude <= Mathf.Epsilon) return false;

        Vector2 directionAwayFromOwnGoal = GetGoalArenaDirection(IsRight, ownGoalMiddle);
        float ownGoalSafetyAlignment = Vector2.Dot(characterToPuk.normalized, directionAwayFromOwnGoal);

        return ownGoalSafetyAlignment >= NPCSettings.MinimumSafeOwnGoalStrikeAlignment;
    }

    float GetStrikeAlignmentThreshold(bool isEmergencyBlock, bool currentlySafeToStrike)
    {
        float requiredAlignment = isEmergencyBlock
            ? NPCSettings.EmergencyClearAlignment
            : NPCSettings.RequiredShotAlignment;
        float stabilityOffset = currentlySafeToStrike
            ? -NPCSettings.AlignmentStabilityMargin
            : NPCSettings.AlignmentStabilityMargin;

        return Mathf.Clamp(requiredAlignment + stabilityOffset, -1f, 1f);
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
            float interceptTime = shotPlan.TimeToGoalLine * i / interceptSamples;
            Vector2 interceptPosition = pukPosition + pukVelocity * interceptTime;
            float reachableDistance = GetBaseMaximumMoveSpeed() * interceptTime + agent.radius;

            if (Vector2.Distance(characterPosition, interceptPosition) <= reachableDistance)
            {
                return interceptPosition;
            }
        }

        return shotPlan.PredictedGoalCrossing;
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
        approachDebug.ValidCandidates.Clear();
        approachDebug.RejectedCandidates.Clear();

        Vector2 idealApproachDirection = -shotPlan.Direction;
        float minimumPukClearance = GetCombinedPukClearance();
        float desiredApproachDistance = Mathf.Max(NPCSettings.PukApproachDistance, minimumPukClearance);
        bool isEmergencyBlock = shotPlan.CurrentIntent == PukIntent.EmergencyBlock;
        float approachAlignment = GetStrikeAlignmentThreshold(isEmergencyBlock, false);
        float maximumAngle = Mathf.Acos(Mathf.Clamp(approachAlignment, -1f, 1f)) * Mathf.Rad2Deg;
        float bestScore = float.PositiveInfinity;
        Vector2 bestPosition = shotPlan.CurrentIntent == PukIntent.EmergencyBlock ? plannedPukPosition + idealApproachDirection * desiredApproachDistance : plannedPukPosition;

        for (int i = 0; i < APPROACH_CANDIDATE_SAMPLE_COUNT; i++)
        {
            float angleAlpha = APPROACH_CANDIDATE_SAMPLE_COUNT == 1 ? 0f : i / (APPROACH_CANDIDATE_SAMPLE_COUNT - 1f);
            float angle = Mathf.Lerp(-maximumAngle, maximumAngle, angleAlpha);
            Vector2 candidateDirection = Quaternion.Euler(0f, 0f, angle) * idealApproachDirection;

            for (int distanceAttempt = 0; distanceAttempt < APPROACH_DISTANCE_ATTEMPTS; distanceAttempt++)
            {
                float distanceAlpha = 1f - distanceAttempt / (float)APPROACH_DISTANCE_ATTEMPTS;
                float candidateDistance = Mathf.Max(desiredApproachDistance * distanceAlpha, minimumPukClearance);
                Vector2 rawCandidate = plannedPukPosition + candidateDirection * candidateDistance;

                if (!TryEvaluateApproachCandidate(rawCandidate, plannedPukPosition, angle, preferClearDashPath, out Vector2 sampledCandidate, out float score))
                {
                    approachDebug.RejectedCandidates.Add(rawCandidate);
                    continue;
                }

                approachDebug.ValidCandidates.Add(sampledCandidate);

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

        bool isEmergencyBlock = shotPlan.CurrentIntent == PukIntent.EmergencyBlock;
        float requiredAlignment = GetStrikeAlignmentThreshold(isEmergencyBlock, false);
        Vector2 sampledShotDifference = plannedPukPosition - sampledCandidate;

        if (sampledShotDifference.sqrMagnitude <= Mathf.Epsilon) return false;
        if (Vector2.Dot(sampledShotDifference.normalized, shotPlan.Direction) < requiredAlignment) return false;
        if (!agent.CalculatePath(navHit.position, approachPlan.CandidatePath)) return false;
        if (approachPlan.CandidatePath.status != NavMeshPathStatus.PathComplete) return false;

        float pathLength = GetPathLength(approachPlan.CandidatePath);
        float anglePenalty = Mathf.Abs(angle) / HALF_ROTATION_DEGREES * NPCSettings.PukApproachDistance;
        float distancePenalty = Mathf.Abs(NPCSettings.PukApproachDistance - Vector2.Distance(plannedPukPosition, sampledCandidate)) * APPROACH_DISTANCE_PENALTY_WEIGHT;
        score = pathLength + anglePenalty + distancePenalty;

        if (movementProgress.IsRecovering)
        {
            float failedTargetDistance = Vector2.Distance(sampledCandidate, movementProgress.FailedTarget);
            float failedTargetPenalty = Mathf.Max(0f, NPCSettings.StuckRecoveryRadius - failedTargetDistance);
            score += failedTargetPenalty * NPCSettings.StuckCandidatePenalty;
        }

        bool hasClearPukPath = true;
        if (preferClearDashPath)
        {
            hasClearPukPath = IsCalculatedPathClearOfPuk(approachPlan.CandidatePath);
        }

        bool shouldPenalizeBlockedPath = preferClearDashPath && !hasClearPukPath;

        if (shouldPenalizeBlockedPath)
        {
            score += BLOCKED_PUK_PATH_SCORE_PENALTY;
        }

        return true;
    }

    void UpdateMovementProgress()
    {
        bool usesActiveMovement = arenaMode == ArenaMode.ToArena || arenaMode == ArenaMode.Arena;
        bool canEvaluateMovement = usesActiveMovement && agent.isOnNavMesh && agent.hasPath && !agent.pathPending && !dashController.IsDashing;
        bool agentWantsToMove = canEvaluateMovement && agent.desiredVelocity.magnitude >= NPCSettings.StuckDesiredSpeed;
        bool isAwayFromTarget = Vector2.Distance(transform.position, targetPos) > agent.stoppingDistance + MIN_NAVMESH_SAMPLE_RADIUS;

        if (!agentWantsToMove || !isAwayFromTarget)
        {
            ResetMovementSample();
            return;
        }

        if (!movementProgress.HasSample)
        {
            movementProgress.SamplePosition = transform.position;
            movementProgress.SampleStartTime = Time.time;
            movementProgress.HasSample = true;
            return;
        }

        if (Time.time < movementProgress.SampleStartTime + NPCSettings.StuckDetectionTime) return;

        float physicalProgress = Vector2.Distance(transform.position, movementProgress.SamplePosition);
        if (physicalProgress < NPCSettings.MinimumStuckProgress)
        {
            BeginStuckRecovery();
            return;
        }

        ResetMovementSample();
    }

    void BeginStuckRecovery()
    {
        bool continuesRecovery = movementProgress.IsRecovering;
        movementProgress.FailedTarget = targetPos;
        movementProgress.BlockedDirection = continuesRecovery ? movementProgress.BlockedDirection : agent.desiredVelocity.RemoveZ().normalized;
        movementProgress.DirectionAttempt = continuesRecovery ? movementProgress.DirectionAttempt + 1 : 0;
        movementProgress.IsRecovering = true;
        movementProgress.RecoveryEndTime = Time.time + NPCSettings.StuckRecoveryDuration;
        shotPlan.CanSafelyStrike = false;
        approachPlan.HasPlan = false;
        approachPlan.HasBeenReached = false;
        approachPlan.IsRoutingAroundPuk = false;
        approachPlan.NextPlanTime = 0f;
        agent.ResetPath();
        TryFindStuckRecoveryPosition(out movementProgress.RecoveryPosition);
        targetPos = movementProgress.RecoveryPosition;
        ResetMovementSample();
    }

    bool TryFollowStuckRecovery()
    {
        if (!movementProgress.IsRecovering) return false;

        float recoveryReach = Mathf.Max(NPCSettings.MinimumStuckProgress, MIN_NAVMESH_SAMPLE_RADIUS);
        bool recoveryExpired = Time.time >= movementProgress.RecoveryEndTime;
        bool reachedRecoveryPosition = Vector2.Distance(transform.position, movementProgress.RecoveryPosition) <= recoveryReach;

        if (recoveryExpired || reachedRecoveryPosition)
        {
            movementProgress.IsRecovering = false;
            ResetMovementSample();
            return false;
        }

        targetPos = movementProgress.RecoveryPosition;
        return true;
    }

    bool TryFindStuckRecoveryPosition(out Vector2 recoveryPosition)
    {
        Vector2 characterPosition = transform.position;
        Vector2 escapeDirection = movementProgress.BlockedDirection.sqrMagnitude > Mathf.Epsilon ? -movementProgress.BlockedDirection : Vector2.up;
        recoveryPosition = characterPosition;

        for (int attemptOffset = 0; attemptOffset < STUCK_RECOVERY_DIRECTION_COUNT; attemptOffset++)
        {
            int directionIndex = (movementProgress.DirectionAttempt + attemptOffset) % STUCK_RECOVERY_DIRECTION_COUNT;
            int directionStep = directionIndex == 0 ? 0 : (directionIndex + 1) / 2 * (directionIndex % 2 == 1 ? 1 : -1);
            Vector2 direction = Quaternion.Euler(0f, 0f, directionStep * STUCK_RECOVERY_DIRECTION_STEP) * escapeDirection;
            Vector2 rawCandidate = characterPosition + direction * NPCSettings.StuckRecoveryMoveDistance;

            bool mustRemainInsideArena = arenaMode == ArenaMode.Arena;
            if (mustRemainInsideArena && arenaCollider && !arenaCollider.OverlapPoint(rawCandidate)) continue;
            if (!NavMesh.SamplePosition(rawCandidate, out NavMeshHit navHit, Mathf.Max(agent.radius, MIN_NAVMESH_SAMPLE_RADIUS), agent.areaMask)) continue;
            if (!agent.CalculatePath(navHit.position, approachPlan.CandidatePath)) continue;
            if (approachPlan.CandidatePath.status != NavMeshPathStatus.PathComplete) continue;

            recoveryPosition = navHit.position.RemoveZ();
            movementProgress.DirectionAttempt = directionIndex;
            return true;
        }

        return false;
    }

    void ResetMovementSample()
    {
        movementProgress.HasSample = false;
    }

    bool HasClearDashPath(Vector2 dashTarget)
    {
        return IsSegmentClearOfPuk(transform.position.RemoveZ(), dashTarget);
    }

    bool IsPathClearOfPuk(Vector2 targetPosition)
    {
        if (!agent.CalculatePath(targetPosition, approachPlan.CandidatePath)) return false;
        if (approachPlan.CandidatePath.status != NavMeshPathStatus.PathComplete) return false;

        Vector2 previousPosition = transform.position.RemoveZ();

        foreach (Vector3 corner in approachPlan.CandidatePath.corners)
        {
            Vector2 cornerPosition = corner.RemoveZ();

            if (!IsSegmentClearOfPuk(previousPosition, cornerPosition)) return false;

            previousPosition = cornerPosition;
        }

        return true;
    }

    bool TryFindPukAvoidanceRoutePosition(Vector2 behindPukPosition, out Vector2 routePosition)
    {
        Vector2 pukPosition = Puk.position.RemoveZ();
        Vector2 behindDifference = behindPukPosition - pukPosition;
        Vector2 behindDirection = behindDifference.sqrMagnitude > Mathf.Epsilon ? behindDifference.normalized : -shotPlan.Direction;
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
            if (!agent.CalculatePath(navHit.position, approachPlan.CandidatePath)) continue;
            if (approachPlan.CandidatePath.status != NavMeshPathStatus.PathComplete) continue;
            if (!IsCalculatedPathClearOfPuk(approachPlan.CandidatePath)) continue;
            if (!IsSegmentClearOfPuk(sampledRoutePosition, behindPukPosition)) continue;

            float pathLength = GetPathLength(approachPlan.CandidatePath);

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
        float requiredClearance = GetCombinedPukClearance();
        float startDistance = Vector2.Distance(segmentStart, pukPosition);

        if (segmentDifference.sqrMagnitude <= Mathf.Epsilon)
        {
            // NavMesh paths commonly repeat the current position as their first corner.
            // A zero-length segment cannot move the character farther into the puck.
            return true;
        }

        if (startDistance < requiredClearance)
        {
            Vector2 directionFromPuk = segmentStart - pukPosition;
            float endDistance = Vector2.Distance(segmentEnd, pukPosition);
            bool increasesPukDistance = endDistance > startDistance;
            bool doesNotMoveDeeperIntoPuk = Vector2.Dot(segmentDifference, directionFromPuk) >= 0f;

            return increasesPukDistance && doesNotMoveDeeperIntoPuk;
        }

        float segmentAlpha = Mathf.Clamp01(Vector2.Dot(pukPosition - segmentStart, segmentDifference) / segmentDifference.sqrMagnitude);
        Vector2 closestPoint = segmentStart + segmentDifference * segmentAlpha;
        bool isSegmentClearOfPuk = Vector2.Distance(closestPoint, pukPosition) >= requiredClearance;

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

        return charSO.StoppingDistance;
    }

    public float GetBaseMaximumMoveSpeed()
    {
        EnsureCharSettingsAreCached();

        if (RigidSettings == null) return 0f;

        if (arenaMode != ArenaMode.Arena)
        {
            return RigidSettings.ArenaTransitionMaxSpeed;
        }

        if (!MinigameManager.Instance || !ArenaMiddle)
        {
            return RigidSettings.MiddleLineMaxSpeed;
        }

        if (!MinigameManager.Instance.TryGetGoalMiddle(IsRight, out Vector2 ownGoalMiddle))
        {
            return RigidSettings.MiddleLineMaxSpeed;
        }

        Vector2 goalToMiddle = ArenaMiddle.position.RemoveZ() - ownGoalMiddle;
        float goalToMiddleLengthSquared = goalToMiddle.sqrMagnitude;

        if (goalToMiddleLengthSquared <= Mathf.Epsilon)
        {
            return RigidSettings.MiddleLineMaxSpeed;
        }

        Vector2 goalToCharacter = transform.position.RemoveZ() - ownGoalMiddle;
        float distanceFromGoalToMiddleAlpha = Mathf.Clamp01(Vector2.Dot(goalToCharacter, goalToMiddle) / goalToMiddleLengthSquared);

        return Mathf.Lerp(RigidSettings.OwnGoalMaxSpeed, RigidSettings.MiddleLineMaxSpeed, distanceFromGoalToMiddleAlpha);
    }

    public void SideSettings(bool _isRight)
    {
        IsRight = _isRight;
    }

    public void GoHome()
    {
        SetArenaMode(ArenaMode.Despawn);
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
        sideReturnPlan.IsReturning = false;
        sideReturnPlan.IsRoutingAroundPuk = false;
        SetRigidbodyCollidersEnabled(newMode != ArenaMode.Despawn);
    }

    public void ToArena()
    {
        if (arenaMode != ArenaMode.Arena)
        {
            SetArenaMode(ArenaMode.ToArena);
        }

        defaultTransform = GetRandomDefaultTransform();
    }

    void CacheRigidbodyColliders()
    {
        Rigidbody2D characterRigidbody = GetComponent<Rigidbody2D>();

        foreach (Collider2D attachedCollider in GetComponentsInChildren<Collider2D>(true))
        {
            if (attachedCollider.isTrigger) continue;
            if (attachedCollider.attachedRigidbody != characterRigidbody) continue;

            rigidbodyColliderStates.Add(new RigidbodyColliderState
            {
                Collider = attachedCollider,
                WasEnabled = attachedCollider.enabled,
            });
        }
    }

    void SetRigidbodyCollidersEnabled(bool enabled)
    {
        foreach (RigidbodyColliderState colliderState in rigidbodyColliderStates)
        {
            if (!colliderState.Collider) continue;

            colliderState.Collider.enabled = enabled && colliderState.WasEnabled;
        }
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
        EnsureCharSettingsAreCached();

        Transform[] defaultPositions = IsRight
            ? MinigameManager.Instance.DefaultPosRight
            : MinigameManager.Instance.DefaultPosLeft;

        if (defaultPositions == null || defaultPositions.Length == 0)
        {
            Debug.LogError($"No default positions are configured for {name}.", this);
            return defaultTransform;
        }

        if (NPCSettings == null || Mathf.Approximately(NPCSettings.DefaultPositionGoalBias, 0f))
        {
            return defaultPositions[Random.Range(0, defaultPositions.Length)];
        }

        if (!MinigameManager.Instance.TryGetGoalMiddle(IsRight, out Vector2 ownGoalMiddle))
        {
            return defaultPositions[Random.Range(0, defaultPositions.Length)];
        }

        return GetWeightedDefaultTransform(defaultPositions, ownGoalMiddle, NPCSettings.DefaultPositionGoalBias);
    }

    void EnsureCharSettingsAreCached()
    {
        if (NPCSettings != null) return;

        if (!sOHolder)
        {
            sOHolder = GetComponent<CharSOHolder>();
        }

        if (sOHolder && sOHolder.CharSO)
        {
            CacheCharSettings(sOHolder.CharSO);
        }
    }

    Transform GetWeightedDefaultTransform(Transform[] positions, Vector2 ownGoalPosition, float goalBias)
    {
        Vector2 middlePosition = ArenaMiddle.position;
        float middleToGoalDistance = Vector2.Distance(middlePosition, ownGoalPosition);

        if (middleToGoalDistance <= Mathf.Epsilon)
        {
            return positions[Random.Range(0, positions.Length)];
        }

        float totalWeight = 0f;

        foreach (Transform position in positions)
        {
            totalWeight += GetDefaultPositionWeight(position.position, middlePosition, ownGoalPosition, middleToGoalDistance, goalBias);
        }

        float selection = Random.value * totalWeight;

        foreach (Transform position in positions)
        {
            selection -= GetDefaultPositionWeight(position.position, middlePosition, ownGoalPosition, middleToGoalDistance, goalBias);

            if (selection <= 0f)
            {
                return position;
            }
        }

        return positions[^1];
    }

    float GetDefaultPositionWeight(Vector2 position, Vector2 middlePosition, Vector2 ownGoalPosition, float middleToGoalDistance, float goalBias)
    {
        Vector2 middleToGoalDirection = (ownGoalPosition - middlePosition) / middleToGoalDistance;
        float distanceTowardGoal = Vector2.Dot(position - middlePosition, middleToGoalDirection);
        float goalCloseness = Mathf.Clamp01(distanceTowardGoal / middleToGoalDistance);
        float middleToGoalScore = goalCloseness * 2f - 1f;

        return Mathf.Max(0.001f, 1f + goalBias * middleToGoalScore);
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

        if (!shotPlan.HasPlan) return;

        Vector3 characterPosition = transform.position;
        Vector3 pukPosition = Puk.position;
        Vector3 predictedPosition = shotPlan.PredictedPukPosition;
        Vector3 safeApproachPosition = approachPlan.Position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(pukPosition, 0.3f);
        Gizmos.DrawLine(pukPosition, predictedPosition);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(predictedPosition, 0.3f);

        if (shotPlan.HasPredictedGoalCrossing)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(shotPlan.PredictedGoalCrossing, 0.45f);
            Gizmos.DrawLine(pukPosition, shotPlan.PredictedGoalCrossing);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(shotPlan.DefensiveInterceptPosition, 0.35f);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(safeApproachPosition, 0.4f);
        Gizmos.DrawLine(safeApproachPosition, predictedPosition);

        Gizmos.color = Color.white;

        foreach (Vector2 candidate in approachDebug.ValidCandidates)
        {
            Gizmos.DrawWireSphere(candidate, 0.2f);
        }

        Gizmos.color = new Color(1f, 0.3f, 0.3f);

        foreach (Vector2 candidate in approachDebug.RejectedCandidates)
        {
            Gizmos.DrawWireCube(candidate, Vector3.one * 0.25f);
        }

        Gizmos.color = GetIntentColor();
        DrawArrow(predictedPosition, shotPlan.Direction, 4f);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawLine(pukPosition, ownGoalMiddle);

        Gizmos.color = shotPlan.CanSafelyStrike ? Color.green : Color.red;
        Gizmos.DrawLine(characterPosition, predictedPosition);
        Gizmos.DrawWireSphere(targetPos, 0.5f);
        Gizmos.DrawLine(characterPosition, targetPos);

        if (emergencyPlan.Phase == EmergencyPhase.RouteBehind)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(emergencyPlan.RoutePosition, 0.45f);
            Gizmos.DrawLine(characterPosition, emergencyPlan.RoutePosition);
        }

        if (movementProgress.IsRecovering)
        {
            Gizmos.color = new Color(1f, 0.35f, 0f);
            Gizmos.DrawWireSphere(movementProgress.FailedTarget, NPCSettings.StuckRecoveryRadius);
            Gizmos.DrawWireSphere(movementProgress.RecoveryPosition, 0.4f);
            Gizmos.DrawLine(characterPosition, movementProgress.RecoveryPosition);
        }

#if UNITY_EDITOR
        bool isEmergencyBlock = shotPlan.CurrentIntent == PukIntent.EmergencyBlock;
        float requiredAlignment = GetStrikeAlignmentThreshold(isEmergencyBlock, shotPlan.CanSafelyStrike);
        string movementStatus = shotPlan.DirectlyBlockingThreat ? "DIRECTLY BLOCKING" : shotPlan.CanSafelyStrike ? "SAFE TO CLEAR" : "REPOSITIONING";
        string recoveryStatus = movementProgress.IsRecovering ? "\nSTUCK RECOVERY" : string.Empty;

        if (emergencyPlan.UsesVerticalFallback)
        {
            movementStatus = "VERTICAL LAST-CHANCE CLEAR";
        }

        Handles.Label(pukPosition + Vector3.up * 0.5f, "Puck");
        Handles.Label(predictedPosition + Vector3.up * 0.5f, "Predicted puck");
        Handles.Label(safeApproachPosition + Vector3.up * 0.5f, emergencyPlan.Phase == EmergencyPhase.Backdash ? "Backdash target" : "Safe approach");
        Handles.Label(characterPosition + Vector3.up, $"Intent: {shotPlan.CurrentIntent}\nEmergency: {emergencyPlan.Phase}\nShot: {shotPlan.Alignment:F2} / {requiredAlignment:F2}\nThreat: {shotPlan.OwnGoalThreatAlignment:F2} / {NPCSettings.OwnGoalThreatAlignment:F2}\nGoal distance: {shotPlan.OwnGoalDistance:F1}\nGoal crossing: {(shotPlan.HasPredictedGoalCrossing ? $"{shotPlan.TimeToGoalLine:F2}s" : "None")}\n{movementStatus}{recoveryStatus}");
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
        return shotPlan.CurrentIntent switch
        {
            PukIntent.ClearUp or PukIntent.ClearDown => Color.cyan,
            PukIntent.EmergencyBlock => new Color(1f, 0.5f, 0f),
            _ => Color.green,
        };

    }
}
