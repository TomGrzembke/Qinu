using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary> NPC movement with ArenaMode states </summary>
public class NPCNav : NavCalc
{
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
        approachPosition = predictedPukPosition - shotDirection * PukApproachDistance;

        Vector2 characterToPuk = (predictedPukPosition - transform.position.RemoveZ()).normalized;
        shotAlignment = Vector2.Dot(characterToPuk, shotDirection);
        canSafelyStrike = shotAlignment >= RequiredShotAlignment;
        hasShotPlan = true;

        targetPos = canSafelyStrike ? Puk.position : approachPosition;

        if (canSafelyStrike && DashRandomly && Random.value <= ProbabilityPerFrame)
        {
            moveRB.Dash();
        }
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
