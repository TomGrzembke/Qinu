using UnityEngine;
using UnityEngine.AI;

public class NPCNavMinigame : MonoBehaviour
{
    #region serialized fields
    [SerializeField] Transform target;
    [SerializeField] bool calculateOnly = true;
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

        SetAgentPosition(target.position);

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