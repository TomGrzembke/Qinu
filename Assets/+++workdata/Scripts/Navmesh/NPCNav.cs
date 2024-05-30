using MyBox;
using UnityEngine;

public class NPCNav : NavCalc
{
    public enum ArenaMode
    {
        ToArena,
        Arena,
        Despawn
    }

    #region serialized fields
    [SerializeField] ArenaMode arenaMode;

    [SerializeField] bool isRight;
    [SerializeField] float stoppingDistance = 2;

    [SerializeField] bool goesToDefault = true;
    [SerializeField] bool followBallY = true;
    [SerializeField, ConditionalField(nameof(followBallY))] bool invertY;
    [SerializeField] bool dashRandomly = true;
    [SerializeField] float probabilityPerFrame = 10;

    [SerializeField] MoveRB moveRB;
    [SerializeField] Vector3 targetPos;
    [SerializeField] Collider2D col;

    [SerializeField] Transform defaultTrans;

    Transform Puk => MinigameManager.Instance.Puk;
    Transform ArenaMiddle => MinigameManager.Instance.ArenaMiddle;
    bool PukOnSide => isRight ? ArenaMiddle.position.x < Puk.position.x : ArenaMiddle.position.x > Puk.position.x;
    #endregion

    #region private fields

    #endregion

    void Update()
    {
        if (arenaMode == ArenaMode.ToArena)
            targetPos = defaultTrans.position;
        else if (arenaMode == ArenaMode.Arena)
            InArena();
        else if (arenaMode == ArenaMode.Despawn)
            targetPos = DespawnPos.position;

        SetAgentPosition(targetPos);
        agent.velocity = Vector2.zero;
    }

    void InArena()
    {
        if (PukOnSide)
        {
            targetPos = Puk.position;
            if (dashRandomly)
                if (Random.Range(0, 100) <= probabilityPerFrame)
                    moveRB.Dash();

        }
        else if (!followBallY)
        {
            if (goesToDefault)
                targetPos = defaultTrans.position;
        }
        else
        {
            targetPos.x = defaultTrans.position.x;

            if (!invertY)
                targetPos.y = Puk.position.y;
            else
                targetPos.y = -Puk.position.y;
        }

        if (Vector3.Distance(targetPos, defaultTrans.position) < stoppingDistance)
            targetPos = transform.position;
    }

    public void SideSettings(bool _isRight)
    {
        isRight = _isRight;

        defaultTrans = TournamentManager.Instance.GetRandomDefaultTrans(isRight ? 1 : 0);
    }

    public void GoHome()
    {
        arenaMode = ArenaMode.Despawn;
        SetAgentPosition(DespawnPos);
    }

    void OnDrawGizmosSelected()
    {
    }

    void OnEnable()
    {
        col.enabled = true;
    }
    void OnDisable()
    {
        col.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Arena"))
            arenaMode = ArenaMode.Arena;
    }

    public void SetArenaMode(ArenaMode newMode)
    {
        arenaMode = newMode;
    }
}