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
    [SerializeField] float stoppingDistance = 2;
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


    void Start()
    {
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
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
            if (Vector3.Distance(targetPos, transform.position) < 2)
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

        agent.nextPosition = transform.position;
        SetAgentPosition(targetPos);
    }

    void InArena()
    {
        if (!MinigameManager.Instance) return;

        if (PukOnSide)
        {
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
            if (goesToDefault)
            {
                targetPos = defaultTrans.position;
            }
        }
        else
        {
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

        if (Vector3.Distance(transform.position, targetPos) < stoppingDistance)
        {
            targetPos = transform.position;
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