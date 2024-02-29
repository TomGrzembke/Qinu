using System.Collections;
using UnityEngine;

/// <summary> Flips the object with context</summary>
public class FlipObjectOnVelocity : MonoBehaviour
{
    #region serialized fields
    [SerializeField] float timeToFlip = .3f;
    [SerializeField] AnimationCurve flipCurve;
    #endregion

    #region private fields
    Vector2 targetScale;
    Vector3 localScale;
    bool flipState;
    Rigidbody2D rb;
    float maxScale;
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
        if (rb.velocity.magnitude <= 0) return;

        if (flipRoutine == null)
            flipRoutine = StartCoroutine(Flip());

    }

    IEnumerator Flip()
    {
        flipState = rb.velocity.x > 0;
        localScale = transform.localScale;
        targetScale.y = localScale.y;
        targetScale.x = flipState ? -maxScale : maxScale;

        float flipTime = 0;

        while (timeToFlip > flipTime && flipState == rb.velocity.x > 0)
        {
            flipTime += Time.deltaTime;
            transform.localScale = Vector2.Lerp(localScale, targetScale, flipCurve.Evaluate(flipTime / timeToFlip));

            yield return null;
        }

        flipRoutine = null;
    }
}