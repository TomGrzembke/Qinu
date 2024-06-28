using System.Collections;
using UnityEngine;

public class PunchController : MonoBehaviour
{
    #region Serialized
    [Header("Stinger Info")]

    [SerializeField] Transform handGFX;
    [SerializeField] Transform handTarget;
    [SerializeField] AnimationCurve animationCurve;

    [Header("Time Info")]
    [SerializeField] float timeToAttack = 1;
    [SerializeField] float cooldownForNextAttack = 2;
    [SerializeField] float attackWindupTime = .7f;
    [SerializeField] float percentAlphaWindupAttackLock = 0.4f;

    [SerializeField] float rotationMinus;
    [SerializeField] float defaultRotation;
    #endregion

    #region Non Serialized
    Collider2D targetCol;

    Coroutine attackCoroutine;
    Coroutine cooldownCoroutine;
    #endregion

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!(collision.CompareTag("Puk") || collision.CompareTag("NPC"))) return;

        targetCol = collision;
        AttackTarget(targetCol.transform);

    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Puk"))
            targetCol = null;
    }

    void Update()
    {
        if (targetCol)
            AttackTarget(targetCol.transform);
    }

    public void AttackTarget(Transform target)
    {
        if (cooldownCoroutine == null && attackCoroutine == null)
            attackCoroutine = StartCoroutine(Attack(target));
    }

    public IEnumerator Attack(Transform target)
    {
        float attackTime = 0;
        float currentAttackWindupTime = 0;

        Vector3 currentTargetPos = target.position;
        while (currentAttackWindupTime < attackWindupTime)
        {
            HandFrameRotation(currentAttackWindupTime, target.position);

            currentAttackWindupTime += Time.deltaTime;

            if (attackWindupTime * percentAlphaWindupAttackLock < currentAttackWindupTime)
                currentTargetPos = target.position;
            yield return null;
        }
        Vector3 currentHandPos = handTarget.position;

        while (attackTime < timeToAttack)
        {
            HandFrameRotation(attackTime, target.position);
            attackTime += Time.deltaTime;
            handTarget.position = Vector3.Lerp(currentHandPos, currentTargetPos, animationCurve.Evaluate(attackTime / timeToAttack));
            yield return null;
        }

        cooldownCoroutine = StartCoroutine(Cooldown());
        attackCoroutine = null;
    }

    void HandFrameRotation(float currentTime, Vector3 attackPos)
    {
        Vector3 vectorToTarget = attackPos - handGFX.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - rotationMinus;
        Quaternion newQuaternion = Quaternion.AngleAxis(angle, Vector3.forward);

        handGFX.rotation = Quaternion.Slerp(handGFX.rotation, newQuaternion, currentTime / attackWindupTime);
    }

    public IEnumerator Cooldown()
    {
        float resetTime = 0;
        Vector3 currentPos = handTarget.localPosition;
        Quaternion currentRot = handGFX.localRotation;
        Quaternion rotationTarget;

        while (resetTime < cooldownForNextAttack)
        {
            resetTime += Time.deltaTime;
            float progress = resetTime / cooldownForNextAttack;
            rotationTarget = defaultRotation == 0 ? Quaternion.identity : Quaternion.Euler(0, 0, defaultRotation);

            handTarget.localPosition = Vector3.Lerp(currentPos, Vector2.zero, progress);
            handGFX.localRotation = Quaternion.Slerp(currentRot, rotationTarget, progress);
            yield return null;
        }

        yield return new WaitForSeconds(cooldownForNextAttack - resetTime);

        cooldownCoroutine = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(handGFX.position, handTarget.transform.position);
    }
}