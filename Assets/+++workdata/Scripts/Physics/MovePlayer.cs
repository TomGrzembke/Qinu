using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary> Either depending on agent provided or Input in case of the player</summary>
public class MovePlayer : RBGetter
{
    [SerializeField] bool disableInputRightclick;
    [SerializeField] float outOfReachMinTime = 1;
    [SerializeField] Transform virtualMouseDebug;

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
    Coroutine dashRoutine;
    Coroutine dashCooldownRoutine;

    CharSO charSO;
    float currentOutOfReachTime;

    Vector2 collisionDirection;
    Vector2 virtualMouseOffset;

    List<Vector2> cachedDirections = new();

    [SerializeField] float cachedDirectionAmount = 3;
    [SerializeField] float maxCachedDirectionPercentage = 0.3f;

    [SerializeField] float cursorResetCooldown = 0.1f;
    float currentCursorResetCooldown;

    [SerializeField] private Collider2D extraBallCollider;

    Camera Cam;

    Camera GetCam()
    {
        if (Cam == null) Cam = Camera.main;

        return Cam;
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
        rb.linearVelocity = Vector3.zero;
    }

    Vector2 GetMoveDir()
    {
        if (inputDisabled) return ResetMoveDirection();

        if (currentOutOfReachTime > outOfReachMinTime) return ResetMoveDirection();

        Vector2 direction = GetRawDirection(GetVirtualMousePosition());

        if (direction.sqrMagnitude <= stoppingDistance) return ResetMoveDirection();

        cachedDirections.Add(direction);

        if (cachedDirections.Count > cachedDirectionAmount)
        {
            cachedDirections.Remove(cachedDirections[0]);
        }

        direction = GetAveragedDirection(direction);

        return direction;
    }

    Vector2 ResetMoveDirection()
    {
        cachedDirections.Clear();

        return Vector2.zero;
    }

    Vector2 GetAveragedDirection(Vector2 direction)
    {
        if (cachedDirections.Count == 0) return direction;

        Vector2 cachedAverage = Vector2.zero;

        for (int i = 0; i < cachedDirections.Count; i++)
        {
            cachedAverage += cachedDirections[i];
        }

        cachedAverage /= cachedDirections.Count;

        var blendAlpha = Mathf.Clamp(GetMouseDistanceAlpha(), 0, maxCachedDirectionPercentage);
        direction = Vector2.Lerp(direction, cachedAverage, blendAlpha);

        return direction;
    }

    float GetMouseDistanceAlpha()
    {
        return moveCurve.Evaluate(Vector2.Distance(transform.position, GetVirtualMousePosition()) / maxSpeedDistance);
    }

    void FixedUpdate()
    {
        if (dashRoutine != null) return;

        OutOfReachMonitoring();
        
        VirtualCursorDebug();

        SetBackCursorOnConfined();

        Accelerate();

        CalculateMaxSpeed();

        ClampVelocity();

        if (extraBallCollider != null)
            extraBallCollider.enabled = rb.linearVelocity.magnitude > maxSpeed * 0.85f;
    }

    void VirtualCursorDebug()
    {
        if (virtualMouseDebug == null) return;

        virtualMouseDebug.position = GetVirtualMousePosition();
    }

    void Accelerate()
    {
        Vector2 currentVel = rb.linearVelocity;

        if (GetMoveDir() == Vector2.zero)
        {
            currentVel -= currentVel * decceleration * Time.fixedDeltaTime;

            if (currentVel.magnitude < 0.01f) currentVel = Vector2.zero;
        }
        else
        {
            currentVel += GetMoveDir() * (acceleration / rb.mass) * Time.fixedDeltaTime;
        }

        rb.linearVelocity = currentVel;
    }

    void SetBackCursorOnConfined()
    {
        if (TryFirstVisibleCursorFrame()) return;

        if (Cursor.lockState == CursorLockMode.Confined) return;

        currentCursorResetCooldown -= Time.deltaTime;
        currentCursorResetCooldown = Mathf.Clamp(currentCursorResetCooldown, 0, cursorResetCooldown);

        if (currentOutOfReachTime <= outOfReachMinTime) return;
        if (currentCursorResetCooldown > 0) return;

        currentCursorResetCooldown = cursorResetCooldown;

        virtualMouseOffset = transform.position.RemoveZ() - GetMousePosition();
    }

    bool TryFirstVisibleCursorFrame()
    {
        if (virtualMouseOffset == Vector2.zero) return false;
        if (Cursor.lockState == CursorLockMode.Locked) return false;

        virtualMouseOffset = Vector2.zero;
        return true;
    }

    void OutOfReachMonitoring()
    {
        if (collisionDirection == Vector2.zero)
        {
            currentOutOfReachTime = 0;
            return;
        }

        var relativeMousPos = GetRawDirection(GetVirtualMousePosition());
        var mouseSingleDirection = GetAxisDirection(relativeMousPos);

        // Dot: minus means it points in a different direction, 0 would be perpendicular (rechtwinklich)
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

        if (Cursor.lockState == CursorLockMode.Confined)
        {
            collisionDirection = Vector2.zero;
            return;
        }

        //Makes sure that the player actually moved away from the wall.
        var moveDirection = GetAxisDirection(GetMoveDir());
        if (Vector2.Dot(moveDirection, collisionDirection) >= 0) return;

        collisionDirection = Vector2.zero;
    }

    void CalculateMaxSpeed()
    {
        currentMaxSpeed = Mathf.Lerp(minSpeed, maxSpeed, GetMouseDistanceAlpha());
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Puk")) return;

        var moveDir = GetMoveDir();
        if (moveDir == Vector2.zero) return;

        currentOutOfReachTime = 0;

        collisionDirection = GetSingleAxisDirection(moveDir);
    }

    Vector2 GetSingleAxisDirection(Vector2 input)
    {
        if (Mathf.Abs(input.x) < Mathf.Abs(input.y)) input.x = 0;
        else input.y = 0;

        return GetAxisDirection(input);
    }

    //sign returns -1 if its below 0 and 1 if above
    Vector2 GetAxisDirection(Vector2 input)
    {
        input = input.Clamp(-1, 1);

        if (input.x != 0) input.x = Mathf.Sign(input.x);
        if (input.y != 0) input.y = Mathf.Sign(input.y);

        return input;
    }

    Vector2 GetMousePosition()
    {
        var distortedMouseDelta = InputManager.Instance.GetDistortedMouseDelta();
        var mousePos = GetCam().ScreenToWorldPoint(distortedMouseDelta) - GetCam().transform.position;

        return mousePos;
    }

    Vector2 GetVirtualMousePosition()
    {
        return GetMousePosition() + virtualMouseOffset;
    }

    void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude <= currentMaxSpeed) return;

        rb.linearVelocity = rb.linearVelocity.normalized * currentMaxSpeed;
    }

    public void Dash()
    {
        ResetCollisionConstraint();

        if (dashCooldownRoutine != null) return;
        dashRoutine = StartCoroutine(DashCor());
    }

    void DisableInput(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        inputDisabled = !inputDisabled;
    }

    IEnumerator DashCor()
    {
        if (!dashEnabled) yield break;

        rb.AddForce(GetDashDirection() * dashForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(dashTime);

        dashCooldownRoutine = StartCoroutine(DashCooldown());
        dashRoutine = null;
    }

    Vector2 GetDashDirection()
    {
        if (dashAutomAim) return GetRawDirection(Puk.position).normalized;

        return GetRawDirection(GetVirtualMousePosition()).normalized;
    }

    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);

        dashCooldownRoutine = null;
    }

    /// <summary> Subtracted given vector with transform.pos </summary>
    Vector2 GetRawDirection(Vector3 vecToCompare)
    {
        return vecToCompare - transform.position;
    }
}