using MyBox;
using UnityEngine;

/// <summary> Flips the object with context</summary>
public class FlipObjectOnVelocity : MonoBehaviour
{
    #region serialized fields
    #endregion

    #region private fields
    public bool Flipped => flipped;
    bool flipped;
    bool flipState;
    Vector3 localScale;
    Rigidbody2D rb;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();   
    }

    void Update()
    {
        FlipLogic();
    }

    void FlipLogic()
    {
        localScale = transform.localScale;
        flipState = rb.velocity.x > 0;

        if (flipped == flipState) return;

        transform.localScale = localScale.SetX(-localScale.x);
        flipped = flipState;
    }
}