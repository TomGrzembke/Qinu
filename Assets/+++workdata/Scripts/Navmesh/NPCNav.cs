using System.Collections;
using UnityEngine;

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
                UpdateToArena();
                break;
            case ArenaMode.Arena:
                UpdateArena();
                break;
            case ArenaMode.Despawn:
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
            targetPos = Puk.position;
            return;
        }

        Vector2 predictedPukPosition = Puk.position.RemoveZ() + MinigameManager.Instance.PukRB.linearVelocity * PukPredictionTime;
        Vector2 shotDirection = (opponentGoal.position.RemoveZ() - predictedPukPosition).normalized;
        Vector2 characterToPuk = (predictedPukPosition - transform.position.RemoveZ()).normalized;
        bool canSafelyStrike = Vector2.Dot(characterToPuk, shotDirection) >= RequiredShotAlignment;

        targetPos = canSafelyStrike ? Puk.position : predictedPukPosition - shotDirection * PukApproachDistance;

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
