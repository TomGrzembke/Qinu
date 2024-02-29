using MyBox;
using UnityEngine;

/// <summary> Flips the object with context</summary>
public class FlipObjectOnVelocity : MonoBehaviour
{
    #region serialized fields
    [SerializeField] float flipSpeed = 7;
    #endregion

    #region private fields
    public bool Flipped => flipped;
    bool flipped;
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
        if (rb.velocity.magnitude <= 0) return;

        localScale = transform.localScale;
        flipState = rb.velocity.x > 0;

        transform.localScale = Vector2.Lerp(localScale, localScale.SetX(flipState ? -initialScale : initialScale), Time.deltaTime * flipSpeed);
    }
}