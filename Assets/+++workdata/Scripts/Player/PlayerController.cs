using UnityEngine;

public class PlayerController : RBGetter
{
    #region serialized fields

    [SerializeField] float maxSpeed = 5f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float decceleration = 10f;
    #endregion

    #region private fields
    public Vector2 MoveDir => InputManager.Instance.MovementVec;
    #endregion

    void FixedUpdate()
    {
        Movement();
    }

    void Movement()
    {
        if (MoveDir == Vector2.zero)
        {
            rb.AddForce(rb.velocity * -decceleration, ForceMode2D.Force);
            return;
        }

        rb.AddForce(MoveDir * acceleration, ForceMode2D.Force);

        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, rb.velocity.normalized * maxSpeed, Time.deltaTime * acceleration);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {

    }
}