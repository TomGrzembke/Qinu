using System.Collections;
using UnityEngine;

/// <summary> Flips the object with rb context</summary>
public class FlipObjectOnVelocity : MonoBehaviour
{
    #region serialized fields
    [SerializeField] float timeToFlip = .3f;
    [SerializeField] AnimationCurve flipCurve;
    [SerializeField] float flipSensitivity;
    #endregion

    #region private fields
    Vector3 targetScale;
    Vector3 localScale => transform.localScale;
    bool flipState;
    float maxScale;
    Rigidbody2D rb;
    Coroutine flipRoutine;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        maxScale = transform.localScale.x;
    }

    void Update()
    {
        FlipLogic();
    }

    void FlipLogic()
    {
        if (rb.velocity.magnitude <= flipSensitivity) return;

        flipRoutine ??= StartCoroutine(Flip());
        
    }

    IEnumerator Flip()
    {
        flipState = rb.velocity.x > 0;
        targetScale = localScale;
        targetScale.x = flipState ? -maxScale : maxScale;

        float flipTime = 0;

        while (timeToFlip > flipTime)
        {
            flipTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(localScale, targetScale, flipCurve.Evaluate(flipTime / timeToFlip));
            yield return null;
        }

        flipRoutine = null;
    }
}