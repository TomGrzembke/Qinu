using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

/// <summary> Either depending on agent provided or Input in case of the player</summary>
public class MoveRB : RBGetter
{
    [SerializeField] NavMeshAgent agent;
    
    AnimationCurve moveCurve => charSO.CharSettings.CharRigidSettings.MoveCurve;
    float maxSpeedDistance => charSO.CharSettings.CharRigidSettings.MaxSpeedDistance;
    float maxSpeed => charSO.CharSettings.CharRigidSettings.MaxSpeed;
    float minSpeed => charSO.CharSettings.CharRigidSettings.MinSpeed;
    float stoppingDistance => charSO.CharSettings.CharRigidSettings.StoppingDistance;
    float acceleration => charSO.CharSettings.CharRigidSettings.Acceleration;
    float decceleration => charSO.CharSettings.CharRigidSettings.Decceleration;
    float dashForce => charSO.CharSettings.CharRigidSettings.DashForce;
    float dashTime => charSO.CharSettings.CharRigidSettings.DashTime;
    float dashCooldown => charSO.CharSettings.CharRigidSettings.DashCooldown;
    bool mouseInput => charSO.CharSettings.CharRigidSettings.MouseInput;
    bool dashAutomAim => charSO.CharSettings.CharRigidSettings.DashAutomAim;
    bool dashEnabled => charSO.CharSettings.CharRigidSettings.DashEnabled;

    Transform Puk => MinigameManager.Instance.Puk;
    bool inputDisabled;
    float currentMaxSpeed;
    Coroutine moveRoutine;
    Coroutine dashRoutine;
    Coroutine dashCooldownRoutine;
    CharSO charSO;

    public Vector2 MoveDir
    {
        get
        {
            if (agent != null)
            {
                Vector2 velocity = agent.desiredVelocity.RemoveZ();
                return velocity != Vector2.zero ? velocity.Clamp(-1, 1) : Vector2.zero;
            }
            
            if (!inputDisabled)
            {
                Vector2 direction = InputManager.Instance.MousePos - transform.position.RemoveZ();
                
                if (direction.sqrMagnitude > stoppingDistance * stoppingDistance)
                {
                    return direction;
                }
            }

            return Vector2.zero;
        }
        private set { }
    }

    protected override void AwakeInternal()
    {
        charSO = GetComponent<CharSOHolder>().CharSO;
        currentMaxSpeed = maxSpeed;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        rb.velocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        if (mouseInput)
            currentMaxSpeed = Mathf.Lerp(minSpeed, maxSpeed, moveCurve.Evaluate(Vector2.Distance(transform.position, InputManager.Instance.MousePos) / maxSpeedDistance));
        if (moveRoutine == null && dashRoutine == null)
            moveRoutine = StartCoroutine(Move());
    }

    public void Dash()
    {
        if (dashCooldownRoutine != null) return;
        dashRoutine = StartCoroutine(DashCor());
    }

    public void DisableInput(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        inputDisabled = !inputDisabled;
    }

    IEnumerator Move()
    {
        while (dashRoutine == null)
        {
            if (MoveDir == Vector2.zero)
            {
                rb.AddForce(rb.velocity * -decceleration, ForceMode2D.Force);
                yield return new WaitForFixedUpdate();

                continue;
            }

            rb.AddForce(MoveDir * acceleration, ForceMode2D.Force);

            if (rb.velocity.magnitude > currentMaxSpeed)
            {
                rb.velocity = (rb.velocity * Time.deltaTime).normalized * currentMaxSpeed;
            }

            yield return new WaitForFixedUpdate();

        }
    }

    IEnumerator DashCor()
    {
        if (!dashEnabled) yield break;

        yield return new WaitForFixedUpdate();

        moveRoutine = null;

        if (dashAutomAim)
            rb.AddForce((Puk.position - transform.position).normalized * dashForce, ForceMode2D.Impulse);
        else
            rb.AddForce((InputManager.Instance.MousePos - transform.position.RemoveZ()).Clamp(-1, 1) * dashForce, ForceMode2D.Impulse);

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