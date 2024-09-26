using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenuForRenderPipeline("Custom/BenDayDots", typeof(UniversalRenderPipeline))]
public class BenDayDotsComponent : VolumeComponent, IPostProcessComponent
{
    public FloatParameter threshold = new(.9f, true);
    public FloatParameter intensity = new(1, true);
    public ClampedFloatParameter scatter = new(.7f, 0, 1, true);
    public IntParameter clamp = new (65472, true);
    public ClampedIntParameter maxIterations = new(6, 0, 10);
    public NoInterpColorParameter tint = new(Color.white);

    public IntParameter dotsDensity = new (10, true);
    public ClampedFloatParameter dotsCutoff = new(.4f, 0, 1, true);
    public Vector2Parameter scrollDirection = new(new(), true);

    public bool IsActive()
    {
        return true;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
