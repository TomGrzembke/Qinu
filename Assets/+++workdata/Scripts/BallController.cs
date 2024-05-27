using MyBox;
using UnityEngine;

public class BallController : RBGetter
{
    #region serialized fields
    [SerializeField] float speedStep = 4;
    [SerializeField] float vPointSpeedMax;
    [SerializeField] float vPointSpeedMin;
    [SerializeField] float currentSpeed;
    [SerializeField] int vPoints = 1;
    [SerializeField] int timesToSwitch = 180;
    #endregion

    #region private fields
    [SerializeField] int timesSwitched = 180;
    #endregion

    protected override void AwakeInternal()
    {
    }

    void FixedUpdate()
    {
        currentSpeed = rb.velocity.magnitude;

        if (rb.velocity.magnitude > CalculateSpeedLimit(vPoints))
        {
            rb.velocity = rb.velocity.normalized * CalculateSpeedLimit(vPoints);
        }
        else if (rb.velocity.magnitude < vPointSpeedMin)
        {
            rb.velocity = rb.velocity.normalized * vPointSpeedMin;
        }

        CalculateVPointGroupSwitch(currentSpeed);
    }

    int CalculateVPointGroup(float velocity)
    {
        float vPointGroup = (velocity - 16) / speedStep;
        if (vPointGroup <= 0)
            vPointGroup = 1;

        return vPointGroup.RoundToInt();
    }

    void CalculateVPointGroupSwitch(float velocity)
    {
        if (velocity < vPointSpeedMin && velocity != 0)
            timesSwitched--;
        else if (velocity > vPointSpeedMax)
            timesSwitched--;

        if (timesSwitched > 0) return;

        if (velocity < vPointSpeedMin && velocity != 0)
            vPoints++;
        else if (velocity > vPointSpeedMax)
            vPoints--;

        timesSwitched = timesToSwitch;

    }

    float CalculateSpeedLimit(int vPoint)
    {
        vPointSpeedMax = (vPoint * speedStep) + 16;

        if (vPoint != 1)
            vPointSpeedMin = vPointSpeedMax - speedStep;
        else
            vPointSpeedMin = 0;

        return vPointSpeedMax;
    }
}