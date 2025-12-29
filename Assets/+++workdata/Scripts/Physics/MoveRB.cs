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
        
        Vector2 velocity = agent.desiredVelocity.RemoveZ();
        return velocity != Vector2.zero ? velocity.Clamp(-1, 1) : Vector2.zero;
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
        if (dashRoutine != null) return;

        if (GetMoveDir() == Vector2.zero)
        {
            rb.AddForce(rb.velocity * -decceleration, ForceMode2D.Force);
            return;
        }

        rb.AddForce(GetMoveDir() * acceleration, ForceMode2D.Force);

        if (rb.velocity.magnitude > currentMaxSpeed)
        {
            rb.velocity = (rb.velocity * Time.deltaTime).normalized * currentMaxSpeed;
        }
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
            rb.AddForce((Puk.position - transform.position).normalized * dashForce, ForceMode2D.Impulse);
        else
            rb.AddForce((InputManager.Instance.MousePos - transform.position.RemoveZ()).Clamp(-1, 1) * dashForce,
                ForceMode2D.Impulse);

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