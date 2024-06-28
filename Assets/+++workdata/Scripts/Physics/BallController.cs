using UnityEngine;

/// <summary> Handles the max speed of the ball, is used for speed ups and slow downs </summary>
public class BallController : RBGetter
{
    #region Serialized
    [SerializeField, ShowOnly] float currentSpeed;
    [SerializeField] float maxSpeed;
    #endregion

    protected override void AwakeInternal()
    {
    }

    void FixedUpdate()
    {
        currentSpeed = rb.velocity.magnitude;

        if (rb.velocity.magnitude > maxSpeed)  
            rb.velocity = rb.velocity.normalized * maxSpeed;

    }

    public void AddBallMaxSpeed(float add, bool showImpact = false)
    {
        maxSpeed += add;
        currentSpeed = rb.velocity.magnitude;

        if (!showImpact) return;

        if (currentSpeed + add > 1)
            rb.velocity = rb.velocity.normalized * (currentSpeed + add);
        else
            rb.velocity = rb.velocity.normalized;
    }
}