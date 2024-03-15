using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class MoveRB : RBGetter
{
    #region serialized fields

    [SerializeField] AnimationCurve moveCurve;
    [SerializeField] float maxSpeedDistance;

    [SerializeField] float maxSpeed = 5f;
    [SerializeField] float minSpeed = 1f;
    [SerializeField] float currentMaxSpeed;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float decceleration = 10f;
    [SerializeField] float dashForce = 10f;
    [SerializeField] float dashTime = 0.1f;
    [SerializeField] float dashCooldown = 0.1f;
    [SerializeField] bool dashInput;
    [SerializeField] NavMeshAgent agent;
    Coroutine moveRoutine;
    Coroutine dashRoutine;
    Coroutine dashCooldownRoutine;
    #endregion

    #region private fields
    public Vector2 MoveDir
    {
        get
        {
            if (agent && agent.desiredVelocity != Vector3.zero)
                return agent.desiredVelocity.RemoveZ().Clamp(-1, 1).RoundUp(agent.speed / 10);
            else
                return Vector2.zero;
        }
        private set { }
    }
    #endregion

    protected override void AwakeInternal()
    {
        currentMaxSpeed = maxSpeed;
        InputManager.Instance.SubscribeTo(RightClick, InputManager.Instance.rightClickAction);
    }

    void FixedUpdate()
    {
        currentMaxSpeed = Mathf.Lerp(minSpeed, maxSpeed, moveCurve.Evaluate(Vector2.Distance(transform.position, InputManager.Instance.MousePos) / maxSpeedDistance));
        print(Vector2.Distance(transform.position, InputManager.Instance.MousePos) / maxSpeedDistance);
        if (moveRoutine == null && dashRoutine == null)
            moveRoutine = StartCoroutine(Move());
    }

    void RightClick(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && dashRoutine == null && dashCooldownRoutine == null && dashInput)
        {
            dashRoutine = StartCoroutine(Dash());
        }
    }

    IEnumerator Move()
    {
        while (dashRoutine == null)
        {
            if (MoveDir == Vector2.zero)
            {
                rb.AddForce(rb.velocity * -decceleration, ForceMode2D.Force);
                yield return null;
                continue;
            }

            rb.AddForce(MoveDir * acceleration, ForceMode2D.Force);

            if (rb.velocity.magnitude > currentMaxSpeed)
            {
                rb.velocity = rb.velocity.normalized * currentMaxSpeed;
            }

            yield return null;
        }
    }

    IEnumerator Dash()
    {
        yield return null;
        moveRoutine = null;
        rb.AddForce(MoveDir * dashForce, ForceMode2D.Impulse);

        if (agent)
            agent.ResetPath();

        yield return new WaitForSeconds(dashTime);

        dashCooldownRoutine = StartCoroutine(DashCooldown());
        dashRoutine = null;
    }

    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);

        dashCooldownRoutine = null;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {

    }
}