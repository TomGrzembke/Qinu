using System.Collections;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary> Either depending on agent provided or Input in case of the player</summary>
public class MovePlayer : RBGetter
{
    [SerializeField] bool disableInputRightclick;
    [SerializeField] float outOfReachValue = 1;
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

    //bool mouseInput => charSO.CharSettings.CharRigidSettings.MouseInput;
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

    Vector2 collisionPosition;
    int cachedFlipSideX;
    int cachedFlipSideY;

    Vector2 collisionDirection;

    public Vector2 MoveDir
    {
        get
        {
            if (inputDisabled) return Vector2.zero;

            if (currentOutOfReachTime > outOfReachMinTime) return Vector2.zero;

            Vector2 direction = InputManager.Instance.MousePos - transform.position.RemoveZ();

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

        if (MoveDir == Vector2.zero)
        {
            rb.AddForce(rb.velocity * -decceleration, ForceMode2D.Force);
            return;
        }

        rb.AddForce(MoveDir * acceleration, ForceMode2D.Force);

        CalculateMaxSpeed();

        ClampVelocity();
    }

    private void OutOfReachMonitoring()
    {
        if (collisionPosition == Vector2.zero) return;

        var outOfReach = Vector2.Distance(transform.position, collisionPosition) > outOfReachValue;

        int currentFlipSideX = Mathf.Clamp(transform.position.RemoveZ().x - InputManager.Instance.MousePos.x, -1, 1).RoundToInt();

        if (currentFlipSideX != cachedFlipSideX && cachedFlipSideX != 0)
        {
            currentOutOfReachTime = 0;
            collisionPosition = Vector2.zero;
            cachedFlipSideX = 0;

            return;
        }

        cachedFlipSideX = currentFlipSideX;
        
        int currentFlipSideY = Mathf.Clamp(transform.position.RemoveZ().y - InputManager.Instance.MousePos.y, -1, 1).RoundToInt();
        if (currentFlipSideY != cachedFlipSideY && cachedFlipSideY != 0)
        {
            currentOutOfReachTime = 0;
            collisionPosition = Vector2.zero;
            cachedFlipSideY = 0;

            return;
        }
        
        cachedFlipSideY = currentFlipSideY;
        
        if (outOfReach)
        {
            currentOutOfReachTime = 0;
            collisionPosition = Vector2.zero;
            return;
        }

        currentOutOfReachTime += Time.deltaTime;
    }

    void CalculateMaxSpeed()
    {
        var mouseDistanceAlpha =
            Vector2.Distance(transform.position, InputManager.Instance.MousePos) / maxSpeedDistance;
        currentMaxSpeed = Mathf.Lerp(minSpeed, maxSpeed, moveCurve.Evaluate(mouseDistanceAlpha));
    }

    void ClampVelocity()
    {
        if (rb.velocity.magnitude <= currentMaxSpeed) return;

        rb.velocity = rb.velocity.normalized * currentMaxSpeed;
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (collisionPosition != Vector2.zero) return;

        collisionPosition = transform.position;

        collisionDirection = MoveDir.Clamp(-1, 1);

        if (Mathf.Abs(collisionDirection.x)  < Mathf.Abs(collisionDirection.y))
        {
            collisionDirection.x = 0;
        }
        else
        {
            collisionDirection.y = 0;
        }
    }
}