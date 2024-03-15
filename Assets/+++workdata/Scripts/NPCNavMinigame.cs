using UnityEngine;
using UnityEngine.AI;

public class NPCNavMinigame : MonoBehaviour
{
    #region serialized fields
    [SerializeField] Transform puk;
    [SerializeField] bool calculateOnly = true;
    [SerializeField] bool followBallY = true;
    [SerializeField] bool goesToDefault = true;
    [SerializeField] Transform arenaMiddle;
    [SerializeField] Transform defaultPos;
    [SerializeField] Vector3 targetPos;
    bool PukInReach => arenaMiddle.position.x < puk.position.x;
    #endregion

    #region private fields

    #endregion

    #region serialized fields

    #endregion

    #region private fields
    NavMeshAgent agent;
    #endregion

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

    }

    void Update()
    {
        if (PukInReach)
        {
            targetPos = puk.position;
        }
        else if (!followBallY)
        {
            if (goesToDefault)
                targetPos = defaultPos.position;
        }
        else
        {
            targetPos.x = defaultPos.position.x;
            targetPos.y = puk.position.y;
        }

        SetAgentPosition(targetPos);

        if (calculateOnly)
            agent.velocity = Vector2.zero;
    }

    public void SetAgentPosition(Vector3 targetPos)
    {
        agent.SetDestination(targetPos);

        if (calculateOnly)
            agent.velocity = Vector2.zero;
    }

    void OnDrawGizmosSelected()
    {
    }
}