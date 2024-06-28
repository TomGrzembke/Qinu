using UnityEngine;

public class RBLimiter : RBGetter
{
    #region serialized fields
    [SerializeField] float speedLimit;
    [SerializeField] float currentSpeed;
    #endregion

    #region private fields

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