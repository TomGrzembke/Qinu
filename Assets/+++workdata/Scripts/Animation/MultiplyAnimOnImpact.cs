using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplyAnimOnImpact : MonoBehaviour
{
    #region serialized fields
    [SerializeField] Animator anim;
    [SerializeField] float multiplier = 2;
    [SerializeField] float duration = 2;
    #endregion

    #region private fields
    Coroutine speedRoutine;
    ContactFilter2D contactFilter;
    Collider2D col;
    List<Collider2D> colliders;
    #endregion

    void Awake()
    {
        col = GetComponent<Collider2D>();
        contactFilter.useTriggers = false;
        contactFilter.useLayerMask = true;
        contactFilter.layerMask = LayerMask.NameToLayer("Creature");
        colliders = new();
    }

    void Update()
    {
        if (Physics2D.OverlapCollider(col, contactFilter, colliders) <= 0) return;

        if (speedRoutine == null)
            speedRoutine = StartCoroutine(DoubleSpeedCor());
    }

    IEnumerator DoubleSpeedCor()
    {
        float timeWentBy = 0;
        float initialSpeed = anim.speed;

        anim.speed = initialSpeed * multiplier;

        while (timeWentBy < duration)
        {
            anim.speed = Mathf.Lerp(initialSpeed * multiplier, initialSpeed, timeWentBy / duration);
            timeWentBy += Time.deltaTime;
            yield return null;
        }

        anim.speed = initialSpeed;
        speedRoutine = null;
    }
}