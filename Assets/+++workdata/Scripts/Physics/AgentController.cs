using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    #region Serialized
    [SerializeField] bool calculateOnly;
    #endregion

    #region Non Serialized
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