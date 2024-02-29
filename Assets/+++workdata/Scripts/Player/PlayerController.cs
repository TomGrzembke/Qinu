using MyBox;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region serialized fields

    [SerializeField] float maxSpeed = 5f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float decceleration = 10f;
    #endregion

    #region private fields
    public Vector2 MoveDir => InputManager.Instance.MovementVec;
    Rigidbody2D rb;
    #endregion

    void Awake()  => rb = GetComponent<Rigidbody2D>();
    

    void FixedUpdate()
    {
        Movement();
    }

    void Movement()
    {
        if (MoveDir == Vector2.zero)
        {
            rb.AddForce(rb.velocity * - decceleration, ForceMode2D.Force);
            return;
        }

        rb.AddForce(MoveDir * acceleration, ForceMode2D.Force);
       
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, rb.velocity.normalized * maxSpeed, Time.deltaTime * acceleration);
        }

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