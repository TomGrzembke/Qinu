using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary> Either depending on agent provided or Input in case of the player</summary>
public class MoveRB : RBGetter
{
    const float arrivalTolerance = 0.05f;

    [SerializeField] NavMeshAgent agent;

    float maxSpeed => charSO.CharSettings.CharRigidSettings.MaxSpeed;
    float acceleration => charSO.CharSettings.CharRigidSettings.Acceleration;
    float velocityCorrection => charSO.CharSettings.CharRigidSettings.VelocityCorrection;
    float maxCorrectionAcceleration => charSO.CharSettings.CharRigidSettings.MaxCorrectionAcceleration;
    float dashForce => charSO.CharSettings.CharRigidSettings.DashForce;
    float dashTime => charSO.CharSettings.CharRigidSettings.DashTime;
    float dashCooldown => charSO.CharSettings.CharRigidSettings.DashCooldown;
    bool dashAutomAim => charSO.CharSettings.CharRigidSettings.DashAutomAim;
    bool dashEnabled => charSO.CharSettings.CharRigidSettings.DashEnabled;

    Transform Puk => MinigameManager.Instance.Puk;
    Coroutine dashRoutine;
    Coroutine dashCooldownRoutine;
    NPCCharSO charSO;

    public Vector2 GetMoveDir()
    {
        if (agent == null) return Vector2.zero;

        return agent.desiredVelocity.RemoveZ().normalized;
    }

    protected override void AwakeInternal()
    {
        charSO = (NPCCharSO)GetComponent<CharSOHolder>().CharSO;

        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        UpdateAgentSettings();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        dashRoutine = null;
        dashCooldownRoutine = null;
        rb.linearVelocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        UpdateAgentSettings();

        if (dashRoutine != null) return;

        if (!agent.isOnNavMesh) return;

        agent.nextPosition = rb.position;

        Vector2 desiredVelocity = Vector2.ClampMagnitude(agent.desiredVelocity.RemoveZ(), maxSpeed);
        Vector2 velocityDifference = desiredVelocity - rb.linearVelocity;

        if (!agent.pathPending && (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + arrivalTolerance))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 correctionAcceleration = velocityDifference * velocityCorrection;
        correctionAcceleration = Vector2.ClampMagnitude(correctionAcceleration, maxCorrectionAcceleration);

        rb.AddForce(correctionAcceleration * rb.mass, ForceMode2D.Force);

        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
    }

    void UpdateAgentSettings()
    {
        agent.speed = maxSpeed;
        agent.acceleration = acceleration / rb.mass;
    }

    public void Dash()
    {
        if (!CanDash()) return;

        dashRoutine = StartCoroutine(DashCor(Puk));
    }

    public void DashAtDirection(Transform target)
    {
        if (!CanDash()) return;

        dashRoutine = StartCoroutine(DashCor(target));
    }

    public void DashAtPosition(Vector3 position)
    {
        if (!CanDash()) return;

        dashRoutine = StartCoroutine(DashCor(position));
    }

    bool CanDash()
    {
        if (!dashEnabled) return false;
        if (dashCooldownRoutine != null) return false;
        if (dashRoutine != null) return false;

        return true;
    }

    IEnumerator DashCor(Transform target)
    {
        yield return new WaitForFixedUpdate();

        if (dashAutomAim)
        {
            rb.AddForce((target.position - transform.position).normalized * dashForce, ForceMode2D.Impulse);
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

    IEnumerator DashCor(Vector3 target)
    {
        yield return new WaitForFixedUpdate();

        if (dashAutomAim)
        {
            rb.AddForce((target - transform.position).normalized * dashForce, ForceMode2D.Impulse);
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
