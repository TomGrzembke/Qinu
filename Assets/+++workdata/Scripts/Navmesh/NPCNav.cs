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
    bool inArena;

    Transform defaultTrans;
    Transform Puk => MinigameManager.Instance.Puk;
    Transform ArenaMiddle => MinigameManager.Instance.ArenaMiddle;
    bool PukOnSide => isRight ? ArenaMiddle.position.x < Puk.position.x : ArenaMiddle.position.x > Puk.position.x;
    #endregion

    #region private fields

    #endregion

    void Start()
    {
        SideSettings(isRight);
    }

    void Update()
    {
        if (inArena)
            InArena();
        else
            if (Vector3.Distance(targetPos, defaultTrans.position) < 2)
            targetPos = defaultTrans.position;

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
    }

    public void SideSettings(bool _isRight)
    {
        isRight = _isRight;

        defaultTrans = TournamentManager.Instance.GetRandomDefaultTrans(isRight ? 1 : 0);
    }

    public void SetDefaultPos(Transform targetTrans)
    {
        defaultTrans = targetTrans;
    }

    public void GoHome()
    {
        inArena = false;
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
            inArena = true;
    }
}