using System.Collections;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary> Either depending on agent provided or Input in case of the player</summary>
public class MovePlayer : RBGetter
{
    [SerializeField] bool disableInputRightclick;
    [SerializeField] float outOfReachMinTime = 1;

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

    bool dashAutomAim => charSO.CharSettings.CharRigidSettings.DashAutomAim;
    bool dashEnabled => charSO.CharSettings.CharRigidSettings.DashEnabled;

    Transform Puk => MinigameManager.Instance.Puk;
    bool inputDisabled;
    float currentMaxSpeed;
    Coroutine moveRoutine;
    Coroutine dashRoutine;
    Coroutine dashCooldownRoutine;
    CharSO charSO;
    float currentOutOfReachTime;

    Vector2 collisionDirection;

    public Vector2 MoveDir
    {
        get
        {
            if (inputDisabled) return Vector2.zero;

            if (currentOutOfReachTime > outOfReachMinTime)
            {
                return Vector2.zero;
            }

            Vector2 direction = GetMousePosition() - transform.position.RemoveZ();

            if (direction.sqrMagnitude <= stoppingDistance)
            {
                return Vector2.zero;
            }

            return direction;
        }
        private set { }
    }

    protected override void AwakeInternal()
    {
        charSO = GetComponent<CharSOHolder>().CharSO;
        currentMaxSpeed = maxSpeed;

        if (disableInputRightclick)
            InputManager.Instance.SubscribeTo(DisableInput, InputManager.Instance.RightClickAction);
    }

    void OnDisable()
    {
        if (disableInputRightclick)
            InputManager.Instance.DesubscribeTo(DisableInput, InputManager.Instance.RightClickAction);

        StopAllCoroutines();
        rb.velocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        if (dashRoutine != null) return;

        OutOfReachMonitoring();

        SetBackCursorOnConfined();

        if (MoveDir == Vector2.zero)
        {
            rb.AddForce(rb.velocity * -decceleration, ForceMode2D.Force);
            return;
        }

        rb.AddForce(MoveDir * acceleration, ForceMode2D.Force);

        CalculateMaxSpeed();

        ClampVelocity();
    }

    private void SetBackCursorOnConfined()
    {
        if (currentOutOfReachTime <= outOfReachMinTime) return;
        if (Cursor.visible) return;

        Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
        Mouse.current.WarpCursorPosition(screenPosition);
    }

    void OutOfReachMonitoring()
    {
        if (collisionDirection == Vector2.zero) return;
        var relativeMousPos = GetMousePosition() - transform.position.RemoveZ();
        var mouseSingleDirection = GetAxisDirection(relativeMousPos);

        // Dot - means it points in a different direction, 0 would be perpendicular (rechtwinklich)
        if (Vector2.Dot(mouseSingleDirection, collisionDirection) <= 0)
        {
            ResetCollisionConstraint();
            return;
        }

        currentOutOfReachTime += Time.deltaTime;
    }

    private void ResetCollisionConstraint()
    {
        currentOutOfReachTime = 0;
        collisionDirection = Vector2.zero;
    }

    void CalculateMaxSpeed()
    {
        var mouseDistanceAlpha =
            Vector2.Distance(transform.position, GetMousePosition()) / maxSpeedDistance;
        currentMaxSpeed = Mathf.Lerp(minSpeed, maxSpeed, moveCurve.Evaluate(mouseDistanceAlpha));
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Puk")) return;

        if (collisionDirection != Vector2.zero) return;

        collisionDirection = GetSingleAxisDirection(MoveDir);
    }

    Vector2 GetSingleAxisDirection(Vector2 input)
    {
        if (Mathf.Abs(input.x) < Mathf.Abs(input.y))
        {
            input.x = 0;
        }
        else
        {
            input.y = 0;
        }

        return GetAxisDirection(input);
    }

    Vector2 GetAxisDirection(Vector2 input)
    {
        input = input.Clamp(-1, 1);

        input = input.ToVector2Int();

        return input;
    }


    Vector2 GetMousePosition()
    {
        return InputManager.Instance.MousePos;
    }

    void ClampVelocity()
    {
        if (rb.velocity.magnitude <= currentMaxSpeed) return;

        rb.velocity = rb.velocity.normalized * currentMaxSpeed;
    }

    public void Dash()
    {
        ResetCollisionConstraint();

        if (dashCooldownRoutine != null) return;
        dashRoutine = StartCoroutine(DashCor());
    }

    public void DisableInput(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        inputDisabled = !inputDisabled;
    }

    IEnumerator DashCor()
    {
        if (!dashEnabled) yield break;

        yield return new WaitForFixedUpdate();

        if (dashAutomAim)
            rb.AddForce((Puk.position - transform.position).normalized * dashForce, ForceMode2D.Impulse);
        else
            rb.AddForce((InputManager.Instance.MousePos - transform.position.RemoveZ()).Clamp(-1, 1) * dashForce,
                ForceMode2D.Impulse);

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