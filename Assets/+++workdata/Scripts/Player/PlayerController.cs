using UnityEngine;
using UnityEngine.AI;

public class PlayerController : RBGetter
{
    #region serialized fields

    [SerializeField] float maxSpeed = 5f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float decceleration = 10f;
    [SerializeField] NavMeshAgent agent;
    #endregion

    #region private fields
    public Vector2 MoveDir
    {
        get
        {
            if (InputManager.Instance.MovementVec == Vector2.zero)
                return Vector2.zero;

            if (agent == null)
                return InputManager.Instance.MovementVec;
            else
                return agent.desiredVelocity.RemoveZ().Clamp(-1, 1).RoundUp(agent.speed / 10);

        }
        private set { }
    }
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