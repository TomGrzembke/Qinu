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
    

    public bool IsActive()
    {
        return true;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
