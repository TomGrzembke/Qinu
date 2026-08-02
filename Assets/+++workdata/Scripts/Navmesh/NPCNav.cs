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
            Defend();
        }
    }

    void UpdateDespawn()
    {
        targetPos = DespawnPos.position;
    }

    void ChasePuk()
    {
        Transform opponentGoal = IsRight ? MinigameManager.Instance.LeftGoal : MinigameManager.Instance.RightGoal;

        if (!opponentGoal)
        {
            hasShotPlan = false;
            targetPos = Puk.position;
            return;
        }

        predictedPukPosition = Puk.position.RemoveZ() + MinigameManager.Instance.PukRB.linearVelocity * PukPredictionTime;
        shotDirection = (opponentGoal.position.RemoveZ() - predictedPukPosition).normalized;

        Vector2 characterToPuk = (predictedPukPosition - transform.position.RemoveZ()).normalized;
        shotAlignment = Vector2.Dot(characterToPuk, shotDirection);
        canSafelyStrike = shotAlignment >= RequiredShotAlignment;
        hasShotPlan = true;

        if (!canSafelyStrike && (!hasApproachPlan || Time.time >= nextApproachPlanTime))
        {
            approachPosition = FindBestApproachPosition();
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
            moveRB.Dash();
        }
    }

    Vector2 FindBestApproachPosition()
    {
        validApproachCandidates.Clear();
        rejectedApproachCandidates.Clear();

        Vector2 idealApproachDirection = -shotDirection;
        float maximumAngle = Mathf.Acos(Mathf.Clamp(RequiredShotAlignment, -1f, 1f)) * Mathf.Rad2Deg;
        float bestScore = float.PositiveInfinity;
        Vector2 bestPosition = predictedPukPosition;

        for (int i = 0; i < approachCandidateCount; i++)
        {
            float angleAlpha = approachCandidateCount == 1 ? 0f : i / (approachCandidateCount - 1f);
            float angle = Mathf.Lerp(-maximumAngle, maximumAngle, angleAlpha);
            Vector2 candidateDirection = Quaternion.Euler(0f, 0f, angle) * idealApproachDirection;

            for (int distanceAttempt = 0; distanceAttempt < approachDistanceAttempts; distanceAttempt++)
            {
                float distanceAlpha = 1f - distanceAttempt / (float)approachDistanceAttempts;
                Vector2 rawCandidate = predictedPukPosition + candidateDirection * PukApproachDistance * distanceAlpha;

                if (!TryEvaluateApproachCandidate(rawCandidate, angle, out Vector2 sampledCandidate, out float score))
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

    bool TryEvaluateApproachCandidate(Vector2 rawCandidate, float angle, out Vector2 sampledCandidate, out float score)
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
        float distancePenalty = Mathf.Abs(PukApproachDistance - Vector2.Distance(predictedPukPosition, sampledCandidate)) * 0.5f;
        score = pathLength + anglePenalty + distancePenalty;
        return true;
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
        if (!hasShotPlan || !MinigameManager.Instance) return;

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

        Gizmos.color = Color.green;
        DrawArrow(predictedPosition, shotDirection, 4f);

        Gizmos.color = canSafelyStrike ? Color.green : Color.red;
        Gizmos.DrawLine(characterPosition, predictedPosition);
        Gizmos.DrawWireSphere(targetPos, 0.5f);
        Gizmos.DrawLine(characterPosition, targetPos);

#if UNITY_EDITOR
        Handles.Label(pukPosition + Vector3.up * 0.5f, "Puck");
        Handles.Label(predictedPosition + Vector3.up * 0.5f, "Predicted puck");
        Handles.Label(safeApproachPosition + Vector3.up * 0.5f, "Safe approach");
        Handles.Label(characterPosition + Vector3.up, $"Alignment: {shotAlignment:F2} / {RequiredShotAlignment:F2}\n{(canSafelyStrike ? "SAFE TO STRIKE" : "REPOSITIONING")}");
#endif
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
