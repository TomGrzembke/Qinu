using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    #region serialized fields
    [SerializeField] bool calculateOnly;
    #endregion

    #region private fields

    #endregion

    #region serialized fields
    [SerializeField] float range = 5;
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
        SetAgentPosition(InputManager.Instance.MousePos);

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