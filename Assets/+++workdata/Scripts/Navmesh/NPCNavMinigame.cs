using MyBox;
using UnityEngine;

public class NPCNavMinigame : NavCalc
{
    #region serialized fields
    [SerializeField] bool isRight = true;
    [SerializeField] Transform arenaMiddle;
    [SerializeField] Transform defaultPos;

    [SerializeField] Transform puk;
    [SerializeField] bool goesToDefault = true;
    [SerializeField] bool followBallY = true;
    [SerializeField, ConditionalField(nameof(followBallY))] bool invertY;
    [SerializeField] bool dashRandomly = true;
    [SerializeField] float probabilityPerFrame = 10;

    [SerializeField] MoveRB moveRB;
    [SerializeField] Vector3 targetPos;
    [SerializeField] Collider2D col;
    bool PukOnSide => isRight ? arenaMiddle.position.x < puk.position.x : arenaMiddle.position.x > puk.position.x;
    #endregion

    #region private fields

    #endregion

    void Update()
    {
        if (PukOnSide)
        {
            targetPos = puk.position;
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
                targetPos.y = puk.position.y;
            else
                targetPos.y = -puk.position.y;
        }

        SetAgentPosition(targetPos);

        agent.velocity = Vector2.zero;
    }
    public void SideSettings(bool _isRight, Transform _arenaMiddle = null)
    {
        isRight = _isRight;

        if (_arenaMiddle != null)
            arenaMiddle = _arenaMiddle;

        if (defaultPos == null)
            defaultPos = TournamentManager.Instance.GetRandomDefaultTrans(_isRight ? 1 : 0);
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