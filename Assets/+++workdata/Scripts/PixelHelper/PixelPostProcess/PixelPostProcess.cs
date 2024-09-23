using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenuForRenderPipeline("Custom/PixelPost", typeof(UniversalRenderPipeline))]
public class PixelPostProcessComponent : VolumeComponent, IPostProcessComponent
{
    [Header("PixelSettings")]
    public ClampedFloatParameter PixelRes = new(550, 16, 1920, true);
    public NoInterpColorParameter outlineCol = new(Color.white);
    public FloatParameter outlineSize = new(1, true);

    #region BloomSettings
    public FloatParameter threshold = new(.9f, true);
    public FloatParameter intensity = new(1, true);
    public ClampedFloatParameter scatter = new(.7f, 0, 1, true);
    public IntParameter clamp = new IntParameter(65472, true);
    public ClampedIntParameter maxIterations = new(6, 0, 10);
    public NoInterpColorParameter tint = new(Color.white);

    public IntParameter dotsDensity = new IntParameter(10, true);
    public ClampedFloatParameter dotsCutoff = new(.4f, 0, 1, true);
    public Vector2Parameter scrollDirection = new(new());


    #endregion

    public bool IsActive()
    {
        
        return true;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
