using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary> Either depending on agent provided or Input in case of the player</summary>
public class MoveRB : RBGetter
{
    [SerializeField] NavMeshAgent agent;

    float maxSpeed => charSO.CharSettings.CharRigidSettings.MaxSpeed;
    float acceleration => charSO.CharSettings.CharRigidSettings.Acceleration;
    float decceleration => charSO.CharSettings.CharRigidSettings.Decceleration;
    float dashForce => charSO.CharSettings.CharRigidSettings.DashForce;
    float dashTime => charSO.CharSettings.CharRigidSettings.DashTime;
    float dashCooldown => charSO.CharSettings.CharRigidSettings.DashCooldown;
    bool dashAutomAim => charSO.CharSettings.CharRigidSettings.DashAutomAim;
    bool dashEnabled => charSO.CharSettings.CharRigidSettings.DashEnabled;

    Transform Puk => MinigameManager.Instance.Puk;
    float currentMaxSpeed;
    Coroutine dashRoutine;
    Coroutine dashCooldownRoutine;
    CharSO charSO;

    public Vector2 GetMoveDir()
    {
        if (agent == null) return Vector2.zero;

        return agent.desiredVelocity.RemoveZ().normalized;
    }

    protected override void AwakeInternal()
    {
        charSO = GetComponent<CharSOHolder>().CharSO;
        currentMaxSpeed = maxSpeed;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        rb.linearVelocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        if (dashRoutine != null) return;

        agent.speed = currentMaxSpeed;
        agent.acceleration = acceleration / rb.mass;

        Vector2 desiredVelocity = agent.desiredVelocity.RemoveZ();

        desiredVelocity = Vector2.ClampMagnitude(desiredVelocity, currentMaxSpeed);

        Vector2 velocityDifference = desiredVelocity - rb.linearVelocity;

        float forceLimit = desiredVelocity.sqrMagnitude > 0.001f ? acceleration : decceleration;

        Vector2 force = Vector2.ClampMagnitude(velocityDifference * rb.mass / Time.fixedDeltaTime, forceLimit);

        rb.AddForce(force, ForceMode2D.Force);

        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, currentMaxSpeed);
    }

    public void Dash()
    {
        if (dashCooldownRoutine != null) return;
        dashRoutine = StartCoroutine(DashCor());
    }

    IEnumerator DashCor()
    {
        if (!dashEnabled) yield break;

        yield return new WaitForFixedUpdate();

        if (dashAutomAim)
        {
            rb.AddForce((Puk.position - transform.position).normalized * dashForce, ForceMode2D.Impulse);
        }
        else
        {
            rb.AddForce((InputManager.Instance.MousePos - transform.position.RemoveZ()).Clamp(-1, 1) * dashForce, ForceMode2D.Impulse);
        }

        if (agent)
        {
            agent.ResetPath();
        }

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