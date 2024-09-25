using System.Collections.Generic;
using UnityEngine.Rendering.Universal;


[System.Serializable]
public class MultiPassRendererFeature : ScriptableRendererFeature
{
    public List<string> lightModePasses;
    MultiPassPass mainPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(mainPass);
    }

    public override void Create()
    {
        mainPass = new (lightModePasses);
    }
}