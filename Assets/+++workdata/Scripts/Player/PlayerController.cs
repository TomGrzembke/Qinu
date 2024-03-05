using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : RBGetter
{
    #region serialized fields

    [SerializeField] float maxSpeed = 5f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float decceleration = 10f;
    [SerializeField] float dashForce = 10f;
    [SerializeField] NavMeshAgent agent;
    #endregion

    #region private fields
    public Vector2 MoveDir
    {
        get
        {
            if (agent && agent.desiredVelocity != Vector3.zero)
                return agent.desiredVelocity.RemoveZ().Clamp(-1, 1).RoundUp(agent.speed / 10);
            else if (!InputManager.Instance.HasMoveInput)
                return Vector2.zero;
            else if (agent == null)
                return InputManager.Instance.MovementVec;
            else
                return Vector2.zero;
        }
        private set { }
    }
    #endregion

    protected override void AwakeInternal()
    {
        InputManager.Instance.SubscribeTo(RightClick, InputManager.Instance.rightClickAction);
    }

    void FixedUpdate()
    {
        Movement();
    }

    void Movement()
    {
        if (MoveDir == Vector2.zero)
        {
            rb.AddForce(rb.velocity * -decceleration, ForceMode2D.Force);
            return;
        }

        rb.AddForce(MoveDir * acceleration, ForceMode2D.Force);

        if (rb.velocity.magnitude > maxSpeed && !InputManager.Instance.rightClickAction.IsPressed())
        {
            rb.velocity = Vector2.Lerp(rb.velocity, rb.velocity.normalized * maxSpeed, Time.deltaTime * decceleration);
        }
    }

    void RightClick(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            StartCoroutine(Dash());
        }
    }
    IEnumerator Dash()
    {
        yield return null;
        rb.AddForce(MoveDir * dashForce, ForceMode2D.Impulse);

        if (agent)
            agent.ResetPath();
    }
    void OnTriggerEnter2D(Collider2D collision)
    {

    }
}