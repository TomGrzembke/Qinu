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
    Vector2 Movement => InputManager.Instance.MovementVec;
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
        SetAgentPosition();
    }

    public void SetAgentPosition()
    {
        if (Movement == Vector2.zero)
        {
                agent.velocity = Vector2.zero;
            return;
        }

        SetAgentPosition(transform.position.Add(Movement.x * range, Movement.y * range));
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