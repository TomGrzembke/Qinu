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

    int mainTexID;
    RTHandle m_MainTex;
    GraphicsFormat hdrFormat;

    public PixelatePostProcessPass(Material _m_render, Material _m_composite)
    {
        m_render = _m_render;
        m_composite = _m_composite;

        renderPassEvent = (RenderPassEvent)549;

        mainTexID = Shader.PropertyToID("_MainTex");
        m_MainTex = RTHandles.Alloc(mainTexID, name: "_MainTex");

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

        using (new ProfilingScope(cmd, new ProfilingSampler("Pre Pixelate")))
        {
            SetupDef(cmd, cameraColorTarget);
        }

        using (new ProfilingScope(cmd, new ProfilingSampler("Pixelate")))
        {
            SetupPixel(cmd, cameraColorTarget);
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


        //m_DefCom.SetTexture("_OriginalTex", source); //useful when urp sample buffer blit doesnt, display the wanted screen tex
        m_composite.SetTexture("_MainTex", source);
        m_render.SetTexture("_MainTex", source); 

        Blitter.BlitCameraTexture(cmd, source, source, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, m_render, 0);
        RenderingUtils.ReAllocateIfNeeded(ref m_MainTex, desc, FilterMode.Point, TextureWrapMode.Clamp, name: m_MainTex.name);
    }

    void SetupPixel(CommandBuffer cmd, RTHandle source)
    {
        int tw = m_Descriptor.width;
        int th = m_Descriptor.height;

        var desc = GetCompatibleDescriptor(tw, th, hdrFormat);

        RenderingUtils.ReAllocateIfNeeded(ref m_MainTex, desc, FilterMode.Point, TextureWrapMode.Clamp, name: m_MainTex.name);

        Blitter.BlitCameraTexture(cmd, source, m_MainTex, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, m_composite, 0);
    }
}