using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PukRecovery : MonoBehaviour
{
    enum RecoveryState
    {
        Monitoring,
        Recovering,
    }

    [Header("References")]
    [SerializeField] Rigidbody2D pukRigidbody;
    [SerializeField] Transform recoveryTarget;
    [Tooltip("Recovery is suspended while this object is active or while the puck is near it.")]
    [SerializeField] GameObject cage;

    [Header("Stuck Detection")]
    [SerializeField, Min(0f)] float stationarySpeedThreshold = 0.1f;
    [SerializeField, Min(0f)] float stuckDuration = 5f;
    [SerializeField, Min(0f)] float cageExclusionRadius = 3f;

    [Header("Recovery")]
    [SerializeField, Min(0f)] float recoveryDuration = 1f;
    [SerializeField, Min(0f)] float emergencyTeleportDistance = 300f;

    RecoveryState state;
    Coroutine recoveryRoutine;
    float stationaryTime;
    bool simulationWasEnabled;

    void Awake()
    {
        pukRigidbody ??= GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (state == RecoveryState.Recovering || recoveryTarget == null) return;

        if (IsFarOutsideArena())
        {
            TeleportToRecoveryTarget();
            return;
        }

        if (IsProtectedByCage())
        {
            stationaryTime = 0f;
            return;
        }

        bool isStationary = pukRigidbody.linearVelocity.sqrMagnitude
            <= stationarySpeedThreshold * stationarySpeedThreshold;

        if (!isStationary)
        {
            stationaryTime = 0f;
            return;
        }

        stationaryTime += Time.deltaTime;

        if (stationaryTime >= stuckDuration)
        {
            BeginRecovery();
        }
    }

    void OnDisable()
    {
        if (recoveryRoutine != null)
        {
            StopCoroutine(recoveryRoutine);
            recoveryRoutine = null;
        }

        if (state == RecoveryState.Recovering)
        {
            pukRigidbody.simulated = simulationWasEnabled;
        }

        state = RecoveryState.Monitoring;
        stationaryTime = 0f;
    }

    void BeginRecovery()
    {
        stationaryTime = 0f;
        state = RecoveryState.Recovering;
        recoveryRoutine = StartCoroutine(RecoverPuk());
    }

    IEnumerator RecoverPuk()
    {
        StopPukMovement();
        simulationWasEnabled = pukRigidbody.simulated;
        pukRigidbody.simulated = false;

        Vector2 startPosition = pukRigidbody.position;
        float elapsedTime = 0f;

        while (elapsedTime < recoveryDuration)
        {
            elapsedTime += Time.deltaTime;
            float recoveryProgress = recoveryDuration > Mathf.Epsilon
                ? Mathf.Clamp01(elapsedTime / recoveryDuration)
                : 1f;
            float smoothedProgress = Mathf.SmoothStep(0f, 1f, recoveryProgress);

            pukRigidbody.transform.position = Vector2.Lerp(startPosition, recoveryTarget.position, smoothedProgress);
            yield return null;
        }

        pukRigidbody.transform.position = recoveryTarget.position;
        pukRigidbody.simulated = simulationWasEnabled;
        StopPukMovement();

        recoveryRoutine = null;
        state = RecoveryState.Monitoring;
        stationaryTime = 0f;
    }

    bool IsProtectedByCage()
    {
        if (cage == null) return false;
        if (cage.activeInHierarchy) return true;

        float cageDistanceSquared = ((Vector2)cage.transform.position - pukRigidbody.position).sqrMagnitude;
        return cageDistanceSquared <= cageExclusionRadius * cageExclusionRadius;
    }

    bool IsFarOutsideArena()
    {
        float targetDistanceSquared = ((Vector2)recoveryTarget.position - pukRigidbody.position).sqrMagnitude;
        return targetDistanceSquared >= emergencyTeleportDistance * emergencyTeleportDistance;
    }

    void TeleportToRecoveryTarget()
    {
        pukRigidbody.position = recoveryTarget.position;
        StopPukMovement();
        stationaryTime = 0f;
    }

    void StopPukMovement()
    {
        pukRigidbody.linearVelocity = Vector2.zero;
        pukRigidbody.angularVelocity = 0f;
    }

    void OnValidate()
    {
        stationarySpeedThreshold = Mathf.Max(0f, stationarySpeedThreshold);
        stuckDuration = Mathf.Max(0f, stuckDuration);
        cageExclusionRadius = Mathf.Max(0f, cageExclusionRadius);
        recoveryDuration = Mathf.Max(0f, recoveryDuration);
        emergencyTeleportDistance = Mathf.Max(0f, emergencyTeleportDistance);
    }
}
