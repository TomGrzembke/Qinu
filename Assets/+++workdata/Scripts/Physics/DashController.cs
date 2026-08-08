using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody2D), typeof(CharSOHolder))]
public class DashController : RBGetter
{
    enum TargetMode
    {
        MovingTarget,
        FixedPosition,
        Mouse,
    }

    [SerializeField] CharSOHolder charSOHolder;

    NavMeshAgent agent;
    MovePlayer movePlayer;
    CharSO CharSO => charSOHolder.CharSO;
    Coroutine dashRoutine;
    Coroutine cooldownRoutine;

    DashSettings Settings => CharSO.DashSettings;
    Transform Puk => MinigameManager.Instance.Puk;

    public bool IsDashing => dashRoutine != null;
    public event Action DashStarted;
    public event Action DashFinished;

    protected override void AwakeInternal()
    {
        agent = GetComponent<NavMeshAgent>();
        movePlayer = GetComponent<MovePlayer>();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        dashRoutine = null;
        cooldownRoutine = null;
    }

    public void Dash(float multiplier = 1f)
    {
        if (Settings.AutoAim)
        {
            DashAtTarget(Puk, multiplier);
            return;
        }

        StartDash(TargetMode.Mouse, null, Vector2.zero, multiplier);
    }

    public void DashAtTarget(Transform target, float multiplier = 1f)
    {
        if (!target) return;

        StartDash(TargetMode.MovingTarget, target, target.position, multiplier);
    }

    public void DashAtPosition(Vector2 position, float multiplier = 1f)
    {
        StartDash(TargetMode.FixedPosition, null, position, multiplier);
    }

    void StartDash(TargetMode targetMode, Transform target, Vector2 targetPosition, float multiplier)
    {
        if (!CanDash()) return;

        dashRoutine = StartCoroutine(DashRoutine(targetMode, target, targetPosition, multiplier));
        DashStarted?.Invoke();
    }

    bool CanDash()
    {
        if (!Settings.Enabled) return false;
        if (cooldownRoutine != null) return false;
        if (dashRoutine != null) return false;

        return true;
    }

    IEnumerator DashRoutine(TargetMode targetMode, Transform target, Vector2 targetPosition, float multiplier)
    {
        float duration = Settings.Duration;

        if (duration <= Mathf.Epsilon)
        {
            rb.AddForce(GetDirection(targetMode, targetPosition) * Settings.Force * multiplier, ForceMode2D.Impulse);
            yield return new WaitForFixedUpdate();
        }
        else
        {
            float elapsedTime = 0f;
            float previousVelocityPercentage = 0f;

            while (elapsedTime < duration)
            {
                float dashAlpha = elapsedTime / duration;

                if (targetMode == TargetMode.MovingTarget && target && dashAlpha < Settings.TargetTrackingPercentage)
                {
                    targetPosition = target.position;
                }

                if (agent && agent.isOnNavMesh)
                {
                    agent.ResetPath();
                }

                float appliedStepDuration = Mathf.Min(Time.fixedDeltaTime, duration - elapsedTime);
                float nextDashAlpha = (elapsedTime + appliedStepDuration) / duration;
                float velocityPercentage = Settings.VelocityApplication.Evaluate(nextDashAlpha);
                float velocityPercentageStep = velocityPercentage - previousVelocityPercentage;
                float stepForce = Settings.Force * multiplier * velocityPercentageStep / Time.fixedDeltaTime;
                rb.AddForce(GetDirection(targetMode, targetPosition) * stepForce, ForceMode2D.Force);
                previousVelocityPercentage = velocityPercentage;
                elapsedTime += appliedStepDuration;

                yield return new WaitForFixedUpdate();
            }
        }

        dashRoutine = null;
        DashFinished?.Invoke();
        cooldownRoutine = StartCoroutine(CooldownRoutine());
    }

    Vector2 GetDirection(TargetMode targetMode, Vector2 targetPosition)
    {
        if (targetMode == TargetMode.Mouse)
        {
            Vector2 mousePosition = movePlayer ? movePlayer.DashAimPosition : InputManager.Instance.MousePos;
            return (mousePosition - transform.position.RemoveZ()).normalized;
        }

        return (targetPosition - transform.position.RemoveZ()).normalized;
    }

    IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(Settings.Cooldown);
        cooldownRoutine = null;
    }
}
