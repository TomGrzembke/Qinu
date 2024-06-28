using System.Collections;
using UnityEngine;

/// <summary> Uses the x local scale to flip an object if the set sensibility is bigger than the rb.velocity.magnitude </summary>
public class FlipObjectOnVelocity : MonoBehaviour
{
    #region Serialized
    [SerializeField] float timeToFlip = .3f;
    [SerializeField] AnimationCurve flipCurve;
    [SerializeField] float flipSensitivity;
    #endregion

    #region Non Serialized
    public float MaxScale { get; private set; }
    bool FlipState;
    Vector3 localScale;
    Vector3 targetScale;
    Rigidbody2D rb;
    Coroutine flipRoutine;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        MaxScale = transform.localScale.x;
    }

    void Update()
    {
        FlipLogic();
    }

    void FlipLogic()
    {
        if (rb.velocity.magnitude <= flipSensitivity) return;

        //Only start if null
        flipRoutine ??= StartCoroutine(Flip());

    }

    IEnumerator Flip()
    {
        FlipState = rb.velocity.x > 0;
        localScale = transform.localScale;

        targetScale = localScale;
        targetScale.x = FlipState ? -MaxScale : MaxScale;

        float flipTime = 0;

        while (timeToFlip > flipTime)
        {
            flipTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(localScale, targetScale, flipCurve.Evaluate(flipTime / timeToFlip));
            yield return null;
        }

        flipRoutine = null;
    }

    public void SetMaxScale(float newScale)
    {
        MaxScale = newScale;
        transform.localScale = new(newScale, newScale, 1);
        localScale = transform.localScale;
        targetScale = localScale;
    }
}
