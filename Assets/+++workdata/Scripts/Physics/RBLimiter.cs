using UnityEngine;

public class RBLimiter : RBGetter
{
    #region Serialized
    [SerializeField] float speedLimit;
    [SerializeField] float currentSpeed;
    #endregion

    protected override void AwakeInternal()
    {
    }

    void FixedUpdate()
    {
        currentSpeed = rb.linearVelocity.magnitude;
        if (rb.linearVelocity.magnitude > speedLimit)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * speedLimit;
        }
    }
}