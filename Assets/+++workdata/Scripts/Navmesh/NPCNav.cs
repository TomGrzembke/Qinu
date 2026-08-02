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
        if (agent.isOnNavMesh)
        {
            agent.nextPosition = transform.position;
        }

        if (arenaMode == ArenaMode.ToArena)
        {
            if (defaultTransform)
            {
                targetPos = defaultTransform.position;
            }
            if (Vector3.Distance(targetPos, transform.position) < arenaTransitionDistance)
            {
                arenaMode = ArenaMode.Arena;
            }
        }
        else if (arenaMode == ArenaMode.Arena)
        {
            InArena();
        }
        else if (arenaMode == ArenaMode.Despawn)
        {
            targetPos = DespawnPos.position;
        }

        SetAgentPosition(targetPos);
    }

    void InArena()
    {
        if (!MinigameManager.Instance) return;

        if (PukOnSide)
        {
            targetPos = Puk.position;
            if (DashRandomly)
            {
                if (Random.value <= ProbabilityPerFrame)
                {
                    moveRB.Dash();
                }
            }

        }
        else if (!FollowBallY)
        {
            if (GoesToDefault)
            {
                targetPos = defaultTransform.position;
            }
        }
        else
        {
            targetPos.x = defaultTransform.position.x;

            if (!InvertY)
            {
                targetPos.y = Puk.position.y;
            }
            else
            {
                targetPos.y = -Puk.position.y;
            }
        }

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

            if(waitTime <= 0) yield break;

            yield return new WaitForSeconds(waitTime);
            
            defaultTransform = GetRandomDefaultTransform();
        }
    }

    Transform GetRandomDefaultTransform()
    {
        return TournamentManager.Instance.GetRandomDefaultTrans(IsRight ? 1 : 0);
    }
}
