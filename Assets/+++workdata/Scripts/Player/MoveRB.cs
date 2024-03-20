using MyBox;
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
    [SerializeField] float stoppingDistance = 5f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float decceleration = 10f;
    [SerializeField] float dashForce = 10f;
    [SerializeField] float dashTime = 0.1f;
    [SerializeField] float dashCooldown = 0.1f;
    [SerializeField] bool dashInput;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] bool dashAutomAim = true;
    [SerializeField, ConditionalField(nameof(dashAutomAim))] Transform puk;
    Coroutine moveRoutine;
    Coroutine dashRoutine;
    Coroutine dashCooldownRoutine;
    #endregion

    #region private fields
    public Vector2 MoveDir
    {
        get
        {
            if (agent == null && (InputManager.Instance.MousePos - transform.position.RemoveZ()).Clamp(-1, 1).magnitude > stoppingDistance)
                return (InputManager.Instance.MousePos - transform.position.RemoveZ()).Clamp(-1, 1);
            else if (agent != null && agent.desiredVelocity != Vector3.zero)
                return agent.desiredVelocity.RemoveZ().Clamp(-1, 1);
            else
                return Vector2.zero;
        }
        private set { }
    }
    #endregion

    protected override void AwakeInternal()
    {
        currentMaxSpeed = maxSpeed;
        InputManager.Instance.SubscribeTo(Dash, InputManager.Instance.leftclickAction);
    }

    void FixedUpdate()
    {
        currentMaxSpeed = Mathf.Lerp(minSpeed, maxSpeed, moveCurve.Evaluate(Vector2.Distance(transform.position, InputManager.Instance.MousePos) / maxSpeedDistance));
        if (moveRoutine == null && dashRoutine == null)
            moveRoutine = StartCoroutine(Move());
    }

    void Dash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && dashRoutine == null && dashCooldownRoutine == null && dashInput)
        {
            dashRoutine = StartCoroutine(DashCor());
        }
    }
    public void Dash()
    {
        if (dashCooldownRoutine == null)
        {
            dashRoutine = StartCoroutine(DashCor());
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


            if (MoveDir.x == 0)
            {
                rb.AddForce(MoveDir.SetX(0.0001f) * acceleration, ForceMode2D.Force);
            }
            else
            {
                rb.AddForce(MoveDir * acceleration, ForceMode2D.Force);
            }

            if (rb.velocity.magnitude > currentMaxSpeed)
            {
                rb.velocity = rb.velocity.normalized * currentMaxSpeed;
            }

            yield return null;
        }
    }

    IEnumerator DashCor()
    {
        yield return null;
        moveRoutine = null;

        if (dashAutomAim)
            rb.AddForce((puk.position - transform.position).normalized * dashForce, ForceMode2D.Impulse);
        else
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