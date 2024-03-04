using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    #region serialized fields
    [SerializeField] bool wasd = true;
    #endregion

    #region private fields
    
    #endregion

    #region serialized fields
    [SerializeField] float smoothing = 10;
    #endregion

    #region private fields
    Vector2 Movement => InputManager.Instance.MovementVec;
    Vector2 moveSafe;
    NavMeshAgent agent;
    #endregion

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void FixedUpdate()
    {
        SetAgentPosition();
    }

    public void SetAgentPosition()
    {
        if (Movement == Vector2.zero)
        {
            SetAgentPosition(agent.transform.position);
            return;
        }

        moveSafe = Vector2.Lerp(moveSafe, Movement, Time.deltaTime * smoothing);
        SetAgentPosition(transform.position + new Vector3(moveSafe.x, moveSafe.y, 0) * 5);
    }

    public void SetAgentPosition(Vector3 targetPos)
    {
        agent.SetDestination(targetPos);
    }
    void OnDrawGizmosSelected()
    {
    }
}