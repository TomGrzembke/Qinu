using MyBox;
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
    [SerializeField] bool goesToDefault = true;
    [SerializeField] bool dashRandomly = true;
    [SerializeField] float probabilityPerFrame = 10;

    [SerializeField] MoveRB moveRB;
    [SerializeField] Vector3 targetPos;

    [SerializeField] Transform defaultTrans;
    [field: SerializeField] public Transform TopTextTarget { get; private set; }
    [field: SerializeField] public Transform BotTextTarget { get; private set; }

    Transform Puk => MinigameManager.Instance.Puk;
    Transform ArenaMiddle => MinigameManager.Instance.ArenaMiddle;
    bool PukOnSide => IsRight ? ArenaMiddle.position.x < Puk.position.x : ArenaMiddle.position.x > Puk.position.x;
    bool InvertY => sOHolder.CharSO.CharSettings.CharNPCSettings.InvertY;
    bool FollowBallY => sOHolder.CharSO.CharSettings.CharNPCSettings.FollowBallY;
    float stoppingDistance => sOHolder.CharSO.CharSettings.CharRigidSettings.StoppingDistance;


    void Start()
    {
        if (agent.isOnNavMesh)
            agent.Warp(transform.position);
    }

    void Update()
    {
        if (arenaMode == ArenaMode.ToArena)
        {
            agent.stoppingDistance = stoppingDistance;
            if (defaultTrans)
            {
                targetPos = defaultTrans.position;
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
            agent.stoppingDistance = 0f;
            targetPos = Puk.position;
            if (dashRandomly)
            {
                if (Random.Range(0, 100) <= probabilityPerFrame)
                {
                    moveRB.Dash();
                }
            }

        }
        else if (!FollowBallY)
        {
            agent.stoppingDistance = stoppingDistance;
            if (goesToDefault)
            {
                targetPos = defaultTrans.position;
            }
        }
        else
        {
            agent.stoppingDistance = stoppingDistance;
            targetPos.x = defaultTrans.position.x;

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

    public void SideSettings(bool _isRight)
    {
        IsRight = _isRight;

        defaultTrans = TournamentManager.Instance.GetRandomDefaultTrans(IsRight ? 1 : 0);
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
    }
}
