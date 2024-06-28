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
        currentSpeed = rb.velocity.magnitude;
        if (rb.velocity.magnitude > speedLimit)
        {
            rb.velocity = rb.velocity.normalized * speedLimit;
        }
    }
}