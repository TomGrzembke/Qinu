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

    Transform defaultPos;
    Transform Puk => MinigameManager.Instance.Puk;
    Transform ArenaMiddle => MinigameManager.Instance.ArenaMiddle;
    bool PukOnSide => isRight ? ArenaMiddle.position.x < Puk.position.x : ArenaMiddle.position.x > Puk.position.x;
    #endregion

    #region private fields

    #endregion

    void Update()
    {
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
                targetPos = defaultPos.position;
        }
        else
        {
            targetPos.x = defaultPos.position.x;
            if (!invertY)
                targetPos.y = Puk.position.y;
            else
                targetPos.y = -Puk.position.y;
        }
    }

    public void SideSettings(bool _isRight)
    {
        isRight = _isRight;

        defaultPos = TournamentManager.Instance.GetRandomDefaultTrans(_isRight ? 1 : 0);
    }

    public void SetDefaultPos(Transform targetTrans)
    {
        defaultPos = targetTrans;
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
}