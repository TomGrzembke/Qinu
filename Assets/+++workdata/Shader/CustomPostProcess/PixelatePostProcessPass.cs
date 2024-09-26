using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelatePostProcessPass : ScriptableRenderPass
{
    RTHandle cameraColorTarget;
    RTHandle cameraDepthTarget;

    Material m_render;
    Material m_composite;

    PixelPostProcessComponent m_effect;
    RenderTextureDescriptor m_Descriptor;

    const int maxPyramidSize = 16;
    int[] bloomMipUp;
    int[] bloomMipDown;
    RTHandle[] m_BloomMipUp;
    RTHandle[] m_BloomMipDown;
    GraphicsFormat hdrFormat;

    public PixelatePostProcessPass(Material _m_render, Material _m_composite)
    {
        m_render = _m_render;
        m_composite = _m_composite;

        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;


        const FormatUsage usage = FormatUsage.Linear | FormatUsage.Render;
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

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        VolumeStack stack = VolumeManager.instance.stack;
        m_effect = stack.GetComponent<PixelPostProcessComponent>();

        if (!m_effect.AnyPropertiesIsOverridden()) return;

        CommandBuffer cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, new ProfilingSampler("Pre BenDayBloom")))
        {
            SetupDef(cmd, cameraColorTarget);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        m_Descriptor = renderingData.cameraData.cameraTargetDescriptor;
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

    void SetupDef(CommandBuffer cmd, RTHandle source)
    {
        int tw = m_Descriptor.width;
        int th = m_Descriptor.height;

        var desc = GetCompatibleDescriptor(tw, th, hdrFormat);

        RenderingUtils.ReAllocateIfNeeded(ref m_BloomMipDown[0], desc, FilterMode.Point, TextureWrapMode.Clamp, name: m_BloomMipDown[0].name);

        //m_DefCom.SetTexture("_OriginalTex", source); //useful when urp sample buffer blit doesnt, display the wanted screen tex
        m_composite.SetTexture("_OriginalTex", m_BloomMipDown[0]);

        Blitter.BlitCameraTexture(cmd, source, m_BloomMipDown[0], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, m_render, 0);

    }
}