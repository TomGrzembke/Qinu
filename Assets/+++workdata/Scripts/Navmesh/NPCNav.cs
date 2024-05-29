using MyBox;
using UnityEngine;

public class NPCNav : NavCalc
{
    #region serialized fields
    [SerializeField] bool isRight = true;

    [SerializeField] bool goesToDefault = true;
    [SerializeField] bool followBallY = true;
    [SerializeField, ConditionalField(nameof(followBallY))] bool invertY;
    [SerializeField] bool dashRandomly = true;
    [SerializeField] float probabilityPerFrame = 10;

    [SerializeField] MoveRB moveRB;
    [SerializeField] Vector3 targetPos;
    [SerializeField] Collider2D col;

    [SerializeField] bool inArena;
    [SerializeField] Transform defaultTrans;

    Transform Puk => MinigameManager.Instance.Puk;
    Transform ArenaMiddle => MinigameManager.Instance.ArenaMiddle;
    bool PukOnSide => isRight ? ArenaMiddle.position.x < Puk.position.x : ArenaMiddle.position.x > Puk.position.x;
    #endregion

    #region private fields

    #endregion

    void Start()
    {
        SideSettings(isRight);

        SetAgentPosition(defaultTrans);
    }

    void Update()
    {
        if (inArena)
            InArena();

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
                if (Vector3.Distance(targetPos, defaultTrans.position) < 2)
                    targetPos = defaultTrans.position;
        }
        else
        {
            if (Vector3.Distance(targetPos, defaultTrans.position) < 2)
                targetPos.x = defaultTrans.position.x;

            if (!invertY)
                targetPos.y = Puk.position.y;
            else
                targetPos.y = -Puk.position.y;
        }
    }

    public void SideSettings(bool _isRight)
    {
        isRight = _isRight;

        defaultTrans = TournamentManager.Instance.GetRandomDefaultTrans(isRight ? 1 : 0);
    }

    public void GoHome()
    {
        inArena = false;
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
            inArena = true;
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Arena"))
            inArena = false;
    }
}