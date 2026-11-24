using UnityEngine;

[System.Serializable]
public class SlowMoData
{
    public AnimationCurve InSlowMoCurve = AnimationCurve.Linear(0,0,1,1);
    public float InSlowMoBlendTime = .2f;
    public float InSlowMoBlendTimeScale = .8f;

    public AnimationCurve OutSlowMoCurve = AnimationCurve.Linear(0,0,1,1);
    public float OutSlowMoBlendTime = .2f;
}
