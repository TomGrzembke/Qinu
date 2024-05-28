using MyBox;
using UnityEngine;

public class BallController : RBGetter
{
    #region serialized fields
    [SerializeField, ShowOnly] float currentSpeed;
    [SerializeField] float maxSpeed;
    #endregion

    #region private fields
    #endregion

    protected override void AwakeInternal()
    {
    }

    void FixedUpdate()
    {
        currentSpeed = rb.velocity.magnitude;

        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    public void AddBallMaxSpeed(float add)
    {
        maxSpeed += add;
        currentSpeed = rb.velocity.magnitude;

        if (currentSpeed + add > 1)
            rb.velocity = rb.velocity.normalized * (currentSpeed + add);
        else
            rb.velocity = rb.velocity.normalized;
    }
}