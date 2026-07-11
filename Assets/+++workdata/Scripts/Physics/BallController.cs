using UnityEngine;

/// <summary> Handles the max speed of the ball, is used for speed ups and slow downs </summary>
public class BallController : RBGetter
{
    [SerializeField, ShowOnly] float currentSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float sfxSpeedMargin = 20;

    protected override void AwakeInternal()
    {
    }

    void FixedUpdate()
    {
        currentSpeed = rb.linearVelocity.magnitude;

        if (rb.linearVelocity.magnitude > maxSpeed)  
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

    }

    public void AddBallMaxSpeed(float add, bool showImpact = false)
    {
        maxSpeed += add;
        currentSpeed = rb.linearVelocity.magnitude;

        if (!showImpact) return;

        if (currentSpeed + add > 1)
            rb.linearVelocity = rb.linearVelocity.normalized * (currentSpeed + add);
        else
            rb.linearVelocity = rb.linearVelocity.normalized;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null) return;
        if(sfxSpeedMargin > currentSpeed) return;
        SoundManager.Instance.PlaySound(SoundType.BallHit);
    }
}