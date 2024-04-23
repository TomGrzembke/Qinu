using MyBox;
using UnityEngine;

public class NPCNavMinigame : NavCalc
{
    #region serialized fields
    [SerializeField] Transform puk;
    [SerializeField] bool isRight = true;

    [SerializeField] bool goesToDefault = true;
    [SerializeField] bool followBallY = true;
    [SerializeField, ConditionalField(nameof(followBallY))] bool invertY;
    [SerializeField] bool dashRandomly = true;
    [SerializeField] float probabilityPerFrame = 10;

    [SerializeField] MoveRB moveRB;
    [SerializeField] Transform arenaMiddle;
    [SerializeField] Transform defaultPos;
    [SerializeField] Vector3 targetPos;
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

    void OnDrawGizmosSelected()
    {
    }
}