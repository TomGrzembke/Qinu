using MyBox;
using UnityEngine;

/// <summary> Flips the object with context</summary>
public class FlipObjectOnVelocity : MonoBehaviour
{
    #region serialized fields
    [SerializeField] float flipSpeed = 7;
    [SerializeField] AnimationCurve flipCurve;
    #endregion

    #region private fields
    bool flipState;
    Vector3 localScale;
    Rigidbody2D rb;
    float initialScale;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale.x;
    }

    void Update()
    {
        FlipLogic();
    }

    void FlipLogic()
    {
        if (rb.velocity.x == 0) return;

        localScale = transform.localScale;
        flipState = rb.velocity.x > 0;

        print(rb.velocity.magnitude / flipSpeed);
        transform.localScale = Vector2.Lerp(localScale, localScale.SetX(flipState ? -initialScale : initialScale), flipCurve.Evaluate(rb.velocity.magnitude / flipSpeed));
    }
}