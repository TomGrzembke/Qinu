using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary> NPC movement with ArenaMode states </summary>
public class NPCNav : NavCalc
{
    const int approachCandidateCount = 7;
    const int approachDistanceAttempts = 4;
    const float approachReplanInterval = 0.1f;

    public enum ArenaMode
    {
        ToArena,
        Arena,
        Despawn
    }

    enum PukIntent
    {
        AttackGoal,
        ClearUp,
        ClearDown,
        EmergencyBlock
    }

    enum EmergencyPhase
    {
        None,
        Backdash,
        Clear
    }

    [SerializeField] ArenaMode arenaMode;

    [field: SerializeField] public bool IsRight { get; private set; }
    [SerializeField] float arenaTransitionDistance = 2;

    [SerializeField] MoveRB moveRB;
    [SerializeField] Vector3 targetPos;

    [SerializeField] Transform defaultTransform;
    [field: SerializeField] public Transform TopTextTarget { get; private set; }
    [field: SerializeField] public Transform BotTextTarget { get; private set; }

    Vector2 predictedPukPosition;
    Vector2 shotDirection;
    Vector2 approachPosition;
    float shotAlignment;
    bool canSafelyStrike;
    bool hasShotPlan;
    bool hasApproachPlan;
    PukIntent currentPukIntent;
    PukIntent previousPukIntent;
    EmergencyPhase emergencyPhase;
    float ownGoalThreatAlignment;
    float ownGoalDistance;
    float nextApproachPlanTime;
    Collider2D arenaCollider;
    readonly List<Vector2> validApproachCandidates = new();
    readonly List<Vector2> rejectedApproachCandidates = new();
    NavMeshPath candidatePath;

    Transform Puk => MinigameManager.Instance.Puk;
    Transform ArenaMiddle => MinigameManager.Instance.ArenaMiddle;
    NPCCharSO charSO => (NPCCharSO)sOHolder.CharSO;
    bool PukOnSide => IsRight ? ArenaMiddle.position.x < Puk.position.x : ArenaMiddle.position.x > Puk.position.x;
    bool GoesToDefault => charSO.CharSettings.CharNPCSettings.GoesToDefault;
    bool InvertY => charSO.CharSettings.CharNPCSettings.InvertY;
    bool FollowBallY => charSO.CharSettings.CharNPCSettings.FollowBallY;
    bool DashRandomly => charSO.CharSettings.CharNPCSettings.DashRandomly;
    float ProbabilityPerFrame => charSO.CharSettings.CharNPCSettings.ProbabilityPerFrame;
    float PukApproachDistance => charSO.CharSettings.CharNPCSettings.PukApproachDistance;
    float RequiredShotAlignment => charSO.CharSettings.CharNPCSettings.RequiredShotAlignment;
    float PukPredictionTime => charSO.CharSettings.CharNPCSettings.PukPredictionTime;
    float OwnGoalDangerDistance => charSO.CharSettings.CharNPCSettings.OwnGoalDangerDistance;
    float MinimumThreatSpeed => charSO.CharSettings.CharNPCSettings.MinimumThreatSpeed;
    float OwnGoalThreatAlignment => charSO.CharSettings.CharNPCSettings.OwnGoalThreatAlignment;
    float EmergencyGoalDistance => charSO.CharSettings.CharNPCSettings.EmergencyGoalDistance;
    float EmergencyClearAlignment => charSO.CharSettings.CharNPCSettings.EmergencyClearAlignment;
    float EmergencyDashPukClearance => charSO.CharSettings.CharNPCSettings.EmergencyDashPukClearance;
    bool DashDefensively => charSO.CharSettings.CharNPCSettings.DashDefensively;
    float stoppingDistance => charSO.StoppingDistance;


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
                UpdateArena();
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

    void UpdateArena()
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
        if (!MinigameManager.Instance.TryGetGoalMiddle(IsRight, out Vector2 ownGoalMiddle) ||
            !MinigameManager.Instance.TryGetGoalMiddle(!IsRight, out Vector2 opponentGoalMiddle))
        {
            hasShotPlan = false;
            targetPos = Puk.position;
            return;
        }

        predictedPukPosition = Puk.position.RemoveZ() + MinigameManager.Instance.PukRB.linearVelocity * PukPredictionTime;
        predictedPukPosition = ClampToArenaSideOfGoalLine(predictedPukPosition, ownGoalMiddle);
        currentPukIntent = GetPukIntent(ownGoalMiddle);

        if (currentPukIntent == PukIntent.EmergencyBlock && emergencyPhase == EmergencyPhase.None)
        {
            emergencyPhase = EmergencyPhase.Backdash;
        }

        if (currentPukIntent != previousPukIntent)
        {
            hasApproachPlan = false;
            emergencyPhase = currentPukIntent == PukIntent.EmergencyBlock ? EmergencyPhase.Backdash : EmergencyPhase.None;
            previousPukIntent = currentPukIntent;
        }

        shotDirection = GetDesiredShotDirection(currentPukIntent, ownGoalMiddle, opponentGoalMiddle);

        Vector2 alignmentPosition = currentPukIntent == PukIntent.EmergencyBlock ? Puk.position.RemoveZ() : predictedPukPosition;
        Vector2 characterToPuk = (alignmentPosition - transform.position.RemoveZ()).normalized;
        shotAlignment = Vector2.Dot(characterToPuk, shotDirection);
        float requiredAlignment = currentPukIntent == PukIntent.EmergencyBlock ? EmergencyClearAlignment : RequiredShotAlignment;
        canSafelyStrike = shotAlignment >= requiredAlignment;
        hasShotPlan = true;

        if (currentPukIntent == PukIntent.EmergencyBlock)
        {
            UpdateEmergencyBlock();
            return;
        }

        if (!canSafelyStrike && (!hasApproachPlan || Time.time >= nextApproachPlanTime))
        {
            approachPosition = FindBestApproachPosition(predictedPukPosition);
            hasApproachPlan = true;
            nextApproachPlanTime = Time.time + approachReplanInterval;
        }

        if (canSafelyStrike)
        {
            approachPosition = predictedPukPosition - shotDirection * PukApproachDistance;
            hasApproachPlan = false;
        }

        targetPos = canSafelyStrike ? Puk.position : approachPosition;

        if (canSafelyStrike && DashRandomly && Random.value <= ProbabilityPerFrame)
        {
            moveRB.DashAtPosition(Puk.position);
        }
    }

    void UpdateEmergencyBlock()
    {
        if (emergencyPhase == EmergencyPhase.Backdash && canSafelyStrike)
        {
            emergencyPhase = EmergencyPhase.Clear;
            hasApproachPlan = false;
        }

        if (emergencyPhase == EmergencyPhase.Clear)
        {
            approachPosition = Puk.position.RemoveZ() - shotDirection * PukApproachDistance;
            targetPos = Puk.position;
            return;
        }

        if (!hasApproachPlan || Time.time >= nextApproachPlanTime)
        {
            approachPosition = FindBestApproachPosition(Puk.position.RemoveZ(), true);
            hasApproachPlan = true;
            nextApproachPlanTime = Time.time + approachReplanInterval;
        }

        targetPos = approachPosition;

        if (DashDefensively && HasClearDashPath(approachPosition))
        {
            moveRB.DashAtPosition(approachPosition);
        }
    }

    PukIntent GetPukIntent(Vector2 ownGoalMiddle)
    {
        Vector2 puckVelocity = MinigameManager.Instance.PukRB.linearVelocity;
        Vector2 puckToOwnGoal = ownGoalMiddle - Puk.position.RemoveZ();
        ownGoalDistance = puckToOwnGoal.magnitude;
        ownGoalThreatAlignment = puckVelocity.sqrMagnitude > 0f ? Vector2.Dot(puckVelocity.normalized, puckToOwnGoal.normalized) : -1f;

        if (ownGoalDistance <= EmergencyGoalDistance)
        {
            return PukIntent.EmergencyBlock;
        }

        bool isThreat = ownGoalDistance <= OwnGoalDangerDistance &&
                        puckVelocity.magnitude >= MinimumThreatSpeed &&
                        ownGoalThreatAlignment >= OwnGoalThreatAlignment;

        if (!isThreat) return PukIntent.AttackGoal;

        Vector2 clearDirection = GetClearDirection();
        return clearDirection.y >= 0f ? PukIntent.ClearUp : PukIntent.ClearDown;
    }

    Vector2 GetDesiredShotDirection(PukIntent intent, Vector2 ownGoalMiddle, Vector2 opponentGoalMiddle)
    {
        switch (intent)
        {
            case PukIntent.ClearUp:
                return Vector2.up;
            case PukIntent.ClearDown:
                return Vector2.down;
            case PukIntent.EmergencyBlock:
                return GetGoalArenaDirection(IsRight, ownGoalMiddle);
            default:
                return (opponentGoalMiddle - predictedPukPosition).normalized;
        }
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
        float approachAlignment = currentPukIntent == PukIntent.EmergencyBlock ? EmergencyClearAlignment : RequiredShotAlignment;
        float maximumAngle = Mathf.Acos(Mathf.Clamp(approachAlignment, -1f, 1f)) * Mathf.Rad2Deg;
        float bestScore = float.PositiveInfinity;
        Vector2 bestPosition = currentPukIntent == PukIntent.EmergencyBlock ? transform.position : plannedPukPosition;

        for (int i = 0; i < approachCandidateCount; i++)
        {
            float angleAlpha = approachCandidateCount == 1 ? 0f : i / (approachCandidateCount - 1f);
            float angle = Mathf.Lerp(-maximumAngle, maximumAngle, angleAlpha);
            Vector2 candidateDirection = Quaternion.Euler(0f, 0f, angle) * idealApproachDirection;

            for (int distanceAttempt = 0; distanceAttempt < approachDistanceAttempts; distanceAttempt++)
            {
                float distanceAlpha = 1f - distanceAttempt / (float)approachDistanceAttempts;
                Vector2 rawCandidate = plannedPukPosition + candidateDirection * PukApproachDistance * distanceAlpha;

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

        float sampleRadius = Mathf.Max(agent.radius * 0.5f, 0.25f);

        if (!NavMesh.SamplePosition(rawCandidate, out NavMeshHit navHit, sampleRadius, agent.areaMask)) return false;

        sampledCandidate = navHit.position.RemoveZ();

        if (arenaCollider && !arenaCollider.OverlapPoint(sampledCandidate)) return false;
        if (!agent.CalculatePath(navHit.position, candidatePath)) return false;
        if (candidatePath.status != NavMeshPathStatus.PathComplete) return false;

        float pathLength = GetPathLength(candidatePath);
        float anglePenalty = Mathf.Abs(angle) / 180f * PukApproachDistance;
        float distancePenalty = Mathf.Abs(PukApproachDistance - Vector2.Distance(plannedPukPosition, sampledCandidate)) * 0.5f;
        score = pathLength + anglePenalty + distancePenalty;

        if (preferClearDashPath && !HasClearDashPath(sampledCandidate))
        {
            score += 1000f;
        }

        return true;
    }

    bool HasClearDashPath(Vector2 dashTarget)
    {
        Vector2 dashStart = transform.position.RemoveZ();
        Vector2 puckPosition = Puk.position.RemoveZ();
        Vector2 dashDifference = dashTarget - dashStart;

        if (dashDifference.sqrMagnitude <= Mathf.Epsilon) return false;

        float dashAlpha = Mathf.Clamp01(Vector2.Dot(puckPosition - dashStart, dashDifference) / dashDifference.sqrMagnitude);
        Vector2 closestPoint = dashStart + dashDifference * dashAlpha;
        return Vector2.Distance(closestPoint, puckPosition) >= EmergencyDashPukClearance;
    }

    static float GetPathLength(NavMeshPath path)
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
        if (!FollowBallY)
        {
            if (GoesToDefault)
            {
                targetPos = defaultTransform.position;
            }

            return;
        }

        targetPos.x = defaultTransform.position.x;
        targetPos.y = InvertY ? -Puk.position.y : Puk.position.y;
    }

    protected override float GetStoppingDistance()
    {
        if (arenaMode == ArenaMode.Arena && MinigameManager.Instance && PukOnSide)
        {
            return 0f;
        }

        return stoppingDistance;
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
        Gizmos.DrawWireSphere(ownGoalMiddle, OwnGoalDangerDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ownGoalMiddle, EmergencyGoalDistance);

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

#if UNITY_EDITOR
        float requiredAlignment = currentPukIntent == PukIntent.EmergencyBlock ? EmergencyClearAlignment : RequiredShotAlignment;
        Handles.Label(pukPosition + Vector3.up * 0.5f, "Puck");
        Handles.Label(predictedPosition + Vector3.up * 0.5f, "Predicted puck");
        Handles.Label(safeApproachPosition + Vector3.up * 0.5f, emergencyPhase == EmergencyPhase.Backdash ? "Backdash target" : "Safe approach");
        Handles.Label(characterPosition + Vector3.up, $"Intent: {currentPukIntent}\nEmergency: {emergencyPhase}\nShot: {shotAlignment:F2} / {requiredAlignment:F2}\nThreat: {ownGoalThreatAlignment:F2} / {OwnGoalThreatAlignment:F2}\nGoal distance: {ownGoalDistance:F1}\n{(canSafelyStrike ? "SAFE TO CLEAR" : "REPOSITIONING")}");
#endif
    }

    Color GetIntentColor()
    {
        switch (currentPukIntent)
        {
            case PukIntent.ClearUp:
            case PukIntent.ClearDown:
                return Color.cyan;
            case PukIntent.EmergencyBlock:
                return new Color(1f, 0.5f, 0f);
            default:
                return Color.green;
        }
    }

    static void DrawArrow(Vector3 start, Vector2 direction, float length)
    {
        Vector3 end = start + (Vector3)(direction * length);
        Vector2 perpendicular = new(-direction.y, direction.x);
        Vector3 arrowBase = end - (Vector3)(direction * 0.6f);

        Gizmos.DrawLine(start, end);
        Gizmos.DrawLine(end, arrowBase + (Vector3)(perpendicular * 0.3f));
        Gizmos.DrawLine(end, arrowBase - (Vector3)(perpendicular * 0.3f));
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Arena"))
        {
            arenaCollider = collision;
            SetArenaMode(ArenaMode.Arena);
        }
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

    IEnumerator DefaultSwitchRoutine()
    {
        while (charSO.CharSettings.CharNPCSettings.GoesToDefault)
        {
            var waitTime = charSO.CharSettings.CharNPCSettings.DefaultSwitchTime;

            if (waitTime <= 0) yield break;

            yield return new WaitForSeconds(waitTime);

            defaultTransform = GetRandomDefaultTransform();
        }
    }

    Transform GetRandomDefaultTransform()
    {
        return TournamentManager.Instance.GetRandomDefaultTrans(IsRight ? 1 : 0);
    }
}
