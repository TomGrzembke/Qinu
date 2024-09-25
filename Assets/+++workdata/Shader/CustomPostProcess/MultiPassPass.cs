using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MultiPassPass : ScriptableRenderPass
{
    List<ShaderTagId> tags;

    public MultiPassPass(List<string> _tags)
    {
        tags = new List<ShaderTagId>();

        foreach (var tag in _tags)
        {
            tags.Add(new ShaderTagId(tag));
        }

        renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        FilteringSettings filteringSettings = FilteringSettings.defaultValue;

        foreach (var pass in tags)
        {
            DrawingSettings drawingSettings = CreateDrawingSettings(pass,ref renderingData, SortingCriteria.CommonOpaque);
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
        }

        context.Submit();
    }
}