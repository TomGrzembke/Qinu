using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[VolumeComponentMenuForRenderPipeline("Custom/Pixelate", typeof(UniversalRenderPipeline))]
public class PixelPostProcessComponent : VolumeComponent, IPostProcessComponent
{

    public ClampedFloatParameter pixelRes = new(128, 1, 1080, true);
    public ClampedIntParameter lineSize = new(1, 1, 10, true);
    public NoInterpColorParameter lineCol = new(Color.black);
    public NoInterpColorParameter ignoreCol = new(Color.white, true);

    public bool IsActive()
    {
        return true;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}