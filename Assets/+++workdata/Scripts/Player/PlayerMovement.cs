using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMovement : MonoBehaviour
{
    public enum ControlState
    {
        playerControl,
        gameControl,
        stun
    }

    #region serialized fields
    [SerializeField] float sprintSpeed;
    [SerializeField] float smoothing = 10;
    [SerializeField] ControlState controlState;
    [SerializeField] bool isPerformingMove;
    [SerializeField] float speed;
    #endregion

    #region private fields
    Vector2 movement;
    Vector2 moveSafe;
    PlayerInputActions inputActions;
    NavMeshAgent agent;
    #endregion

    void Awake()
    {
        inputActions = new();

        inputActions.Player.Move.performed += ctx => Movement(ctx.ReadValue<Vector2>());
        inputActions.Player.Move.canceled += ctx => Movement(ctx.ReadValue<Vector2>());

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

    }

    void FixedUpdate()
    {
        HandleRotation();
        if (controlState != ControlState.playerControl) return;

        Smoothing();
        SetAgentPosition();
    }

    void Smoothing()
    {
        agent.speed = speed;
    }

    void HandleRotation()
    {
        if (agent.velocity == Vector3.zero) return;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, agent.velocity.normalized);
        float step = smoothing * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, step);
    }

    void Movement(Vector2 direction)
    {
        isPerformingMove = direction != Vector2.zero;

        if (direction != Vector2.zero)
            movement = direction.normalized;
    }

    public void SetAgentPosition()
    {
        if (movement == Vector2.zero) return;

        moveSafe = Vector2.Lerp(moveSafe, movement, Time.deltaTime * smoothing);
        SetAgentPosition(transform.position + new Vector3(moveSafe.x - 0.003f, moveSafe.y, 0) * 5);
    }

    public void SetAgentPosition(Vector3 targetPos)
    {
        agent.SetDestination(targetPos);
    }

    void Sprint(bool condition)
    {

    }

    void OnDrawGizmosSelected()
    {
    }

    #region Setters
    public void SetControlState(ControlState newControlState, bool disableAgent = false)
    {
        controlState = newControlState;

        if (disableAgent)
            agent.enabled = !disableAgent;
    }
    public void ReenableMovement(bool enableAgent = false)
    {
        controlState = ControlState.playerControl;

        if (enableAgent)
            agent.enabled = enableAgent;
    }
    void StunLogic(bool condition)
    {
        if (condition)
            controlState = ControlState.stun;
        else
            controlState = ControlState.playerControl;
    }
    #endregion

    #region OnEnable/Disable
    public void OnEnable()
    {
        inputActions.Enable();
    }

    public void OnDisable()
    {
        inputActions.Disable();
    }
    #endregion
}