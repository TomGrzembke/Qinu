using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

/// <summary> Disclaimer: This code came from a tutorial for Unity2022.3 and was updated using gemini to Unity 6.5</summary>
public class MultiPassPass : ScriptableRenderPass
{
  // 1. Define a container to send the Renderer List into the Render Graph
    private class PassData 
    {
        public RendererListHandle rendererListHandle;
    }

    List<ShaderTagId> tags;

    public MultiPassPass(List<string> _tags)
    {
        tags = new List<ShaderTagId>();
        foreach (var tag in _tags)
        {
            tags.Add(new ShaderTagId(tag));
        }
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        // 2. Fetch required frame context data 
        UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        UniversalLightData lightData = frameData.Get<UniversalLightData>();
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        TextureHandle cameraColor = resourceData.activeColorTexture;
        if (!cameraColor.IsValid()) return;

        // 3. Create drawing and filtering settings
        SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque;
        DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(tags, renderingData, cameraData, lightData, sortingCriteria);
        FilteringSettings filteringSettings = FilteringSettings.defaultValue;

        // 4. Create the RendererList configuration and convert it to a RenderGraph Handle
        var rendererListParams = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);
        RendererListHandle renderListHandle = renderGraph.CreateRendererList(rendererListParams);

        // 5. Build and submit the raster render pass
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("MultiPass Render", out PassData passData))
        {
            // Assign the created handle to our data struct
            passData.rendererListHandle = renderListHandle;

            // Target the active camera color buffer
            builder.SetRenderAttachment(cameraColor, 0, AccessFlags.Write);
            builder.AllowPassCulling(false);

            // 6. Execute the draw step natively inside the graph
            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                // Replaces context.DrawRenderers and context.Submit()
                context.cmd.DrawRendererList(data.rendererListHandle);
            });
        }
    }
}