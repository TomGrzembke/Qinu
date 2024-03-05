using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class AgentController : MonoBehaviour
{
    #region serialized fields
    [SerializeField] bool calculateOnly;
    [SerializeField] bool wasd = true;
    [SerializeField] bool alwaysPathToMouse;
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

        InputManager.Instance.SubscribeTo(ClickMove, InputManager.Instance.leftclickAction);
        InputManager.Instance.SubscribeTo(ClickMove, InputManager.Instance.rightClickAction);
    }

    void Update()
    {
        if (alwaysPathToMouse)
            SetAgentPosition(InputManager.Instance.MousePos);
    
        if (wasd)
            SetAgentPosition();

        if (calculateOnly)
            agent.velocity = Vector2.zero;
    }

    public void ClickMove(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !wasd)
            SetAgentPosition(InputManager.Instance.MousePos);
    }

    public void SetAgentPosition()
    {
        if (!InputManager.Instance.HasMoveInput)
        {
            agent.velocity = Vector2.zero;
            agent.ResetPath();
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