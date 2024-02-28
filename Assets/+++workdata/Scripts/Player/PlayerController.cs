using MyBox;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region serialized fields

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float acceleration = 10f;
    #endregion

    #region private fields
    public Vector2 MoveDir => InputManager.Instance.MovementVec;
    Vector2 currentVelocity;
    Rigidbody2D rb;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Movement();
    }

    void Movement()
    {
        Vector2 targetVelocity = new Vector2(MoveDir.x, MoveDir.y).normalized * moveSpeed;

        currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        if (currentVelocity.magnitude < 0.1f) currentVelocity = Vector2.zero;

        rb.velocity = currentVelocity;

        //Flip();
    }

    void Flip()
    {
        Vector3 localScale = transform.localScale;
        bool facingRight = rb.velocity.x > 0;

        transform.localScale = localScale.SetX(facingRight ? (facingRight ? -localScale.x : localScale.x) : (facingRight ? -localScale.x : localScale.x));

        //if (moveDir.x > 0)
        //    transform.localScale = localScale.SetX(xScale < 0 ? xScale : -xScale);

        //else if (moveDir.x < 0)
        //    transform.localScale = localScale.SetX(xScale > 0 ? -xScale : xScale);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {

    }
}