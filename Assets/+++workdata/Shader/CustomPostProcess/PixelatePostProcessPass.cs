using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

/// <summary> Disclaimer: This code was written by tommy after a tutorial for Unity2022.3 and was updated using gemini to Unity 6.5</summary>
public class PixelatePostProcessPass : ScriptableRenderPass
{
    RTHandle cameraColorTarget;
    RTHandle cameraDepthTarget;

    Material m_render;
    Material m_composite;
    Material m_DefCom;

    PixelPostProcessComponent m_effect;
    RenderTextureDescriptor m_Descriptor;

    int mainTexID;
    RTHandle m_MainTex;
    GraphicsFormat hdrFormat;

    Vector2 screenRatio;
    Vector2 scaling;

    public PixelatePostProcessPass(Material _m_render, Material _m_composite, Material _defComMat)
    {
        m_DefCom = _defComMat;
        m_render = _m_render;
        m_composite = _m_composite;

        renderPassEvent = (RenderPassEvent)549;

        mainTexID = Shader.PropertyToID("_MainTex");
        m_MainTex = RTHandles.Alloc(mainTexID, name: "_MainTex");

        const GraphicsFormatUsage usage = GraphicsFormatUsage.Linear | GraphicsFormatUsage.Render;
        if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, usage))
        {
            hdrFormat = GraphicsFormat.B10G11R11_UFloatPack32;
        }
        else
        {
            hdrFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
            ? GraphicsFormat.R8G8B8A8_SRGB
            : GraphicsFormat.R8G8B8A8_UNorm;
        }
    }
    private class PassData { }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        // 1. Fetch camera data and descriptor (replaces OnCameraSetup)
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        m_Descriptor = cameraData.cameraTargetDescriptor;

        // 2. Volume manager checks
        VolumeStack stack = VolumeManager.instance.stack;
        m_effect = stack.GetComponent<PixelPostProcessComponent>();

        if (m_effect == null || !m_effect.AnyPropertiesIsOverridden())
            return;

        // 3. Fetch active camera color textures
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        TextureHandle cameraColorTarget = resourceData.activeColorTexture;

        if (!cameraColorTarget.IsValid())
            return;

        // 4. Set up Render Graph raster pass (replaces CommandBufferPool.Get & ProfilingScope)
        using (var builder = renderGraph.AddRasterRenderPass("Pixelate", out PassData passData))
        {
            // Tell Render Graph we are reading/writing the camera texture
            builder.SetRenderAttachment(cameraColorTarget, 0, AccessFlags.ReadWrite);
            builder.AllowPassCulling(false);

            // 5. This block handles the actual execution using context.cmd
            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                // Call your internal script calculations using the graph's safe command buffer
                // Make sure these helper methods accept context.cmd if they use a command buffer!
                SetupPixel();
                CalculatePixelScaling();

                // Set material properties
                m_composite.SetVector("_Resolution", scaling);
                m_composite.SetFloat("_OutlineThickness", m_effect.lineSize.value);
                m_composite.SetFloat("_ColormaskRange", m_effect.colorMaskRange.value);
                m_composite.SetFloat("_ColorMaskFuziness", m_effect.colorMaskFuzziness.value);
                m_composite.SetColor("_OutlineColor", m_effect.lineCol.value);
                m_composite.SetColor("_IgnoreCol", m_effect.ignoreCol.value);

                // Render Graph uses standard TextureHandle properties instead of old RTHandles
                m_composite.SetTexture("_MainTex", cameraColorTarget);

                // First Blit (Camera -> Temporary/MainTex)
                // Note: Ensure 'm_MainTex' is updated to be a valid TextureHandle or target in your script
                Blitter.BlitTexture(context.cmd, cameraColorTarget, new Vector4(1, 1, 0, 0), m_composite, 0);

                m_DefCom.SetTexture("_OriginalTex", m_MainTex);

                // Second Blit (Temporary/MainTex -> Camera)
                Blitter.BlitTexture(context.cmd, m_MainTex, new Vector4(1, 1, 0, 0), m_DefCom, 0);
            });
        }
    }

    RenderTextureDescriptor GetCompatibleDescriptor()
        => GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat);

    RenderTextureDescriptor GetCompatibleDescriptor(int width, int heigth, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None)
        => GetCompatibleDescriptor(m_Descriptor, width, heigth, format, depthBufferBits);

    public static RenderTextureDescriptor GetCompatibleDescriptor(RenderTextureDescriptor desc, int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None)
    {
        desc.depthBufferBits = (int)depthBufferBits;
        desc.msaaSamples = 1;
        desc.width = width;
        desc.height = height;
        desc.graphicsFormat = format;
        return desc;
    }

    public void SetTarget(RTHandle cameraColorTargetHandle, RTHandle cameraDepthTargetHandle)
    {
        cameraColorTarget = cameraColorTargetHandle;
        cameraDepthTarget = cameraDepthTargetHandle;
    }

    void SetupPixel()
    {
        var targetWidth = m_Descriptor.width;
        var targetHeight = m_Descriptor.height;

        var desc = GetCompatibleDescriptor(targetWidth, targetHeight, hdrFormat);

        RenderingUtils.ReAllocateHandleIfNeeded(ref m_MainTex, desc, FilterMode.Point, TextureWrapMode.Clamp, name: m_MainTex.name);
    }

    void CalculatePixelScaling()
    {
        float targetWidth = m_Descriptor.width;
        float targetHeight = m_Descriptor.height;
        float greatestCommonFactor = CalculateGCF(targetWidth, targetHeight);

        float pixelResWidth = CalculatePixelRes();

        screenRatio.x = targetWidth / greatestCommonFactor;
        screenRatio.y = targetHeight / greatestCommonFactor;

        scaling.x = pixelResWidth;
        scaling.y = pixelResWidth / screenRatio.x * screenRatio.y;

    }

    /// <summary> Assures that the pixelRes is always even </summary>
    float CalculatePixelRes()
    {
        return m_effect.pixelRes.value % 2 == 0 ? m_effect.pixelRes.value : m_effect.pixelRes.value + 1;
    }

    /// <summary> GCF = Greatest common factor </summary>
    float CalculateGCF(float a, float b)
    {
        if (b == 0)
            return a;

        else
            return CalculateGCF(b, a % b);
    }

    /// <summary> GCF = Greatest common factor </summary>
    float CalculateGCF(Vector2 values)
    {
        if (values.y == 0)
            return values.x;

        else
            return CalculateGCF(values.y, values.x % values.y);
    }
}