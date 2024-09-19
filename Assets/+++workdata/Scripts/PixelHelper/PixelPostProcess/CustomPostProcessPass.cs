using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class CustomPostProcessPass : ScriptableRenderPass
{
    RTHandle cameraColorTarget;
    RTHandle cameraDepthTarget;

    Material m_render;
    Material m_composite;

    PixelPostProcessComponent m_effect;
    RenderTextureDescriptor m_Descriptor;

    #region  blurStuff from tutorial
    const int maxPyramidSize = 16;
    int[] bloomMipUp;
    int[] bloomMipDown;
    RTHandle[] m_BloomMipUp;
    RTHandle[] m_BloomMipDown;
    GraphicsFormat hdrFormat;
    #endregion

    public CustomPostProcessPass(Material _renderMat, Material _compositeMaterial)
    {
        m_render = _renderMat;
        m_composite = _compositeMaterial;

        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        bloomMipUp = new int[maxPyramidSize];
        bloomMipDown = new int[maxPyramidSize];
        m_BloomMipUp = new RTHandle[maxPyramidSize];
        m_BloomMipDown = new RTHandle[maxPyramidSize];

        for (int i = 0; i < maxPyramidSize; i++)
        {
            bloomMipUp[i] = Shader.PropertyToID("_BloomMipUp" + i);
            bloomMipDown[i] = Shader.PropertyToID("_BloomMipDown" + i);
            m_BloomMipUp[i] = RTHandles.Alloc(bloomMipUp[i], name: "_BloomMipUp" + i);
            m_BloomMipDown[i] = RTHandles.Alloc(m_BloomMipDown[i], name: "_BloomMipDown" + i);
        }

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

        CommandBuffer cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, new ProfilingSampler("Custom Post Process Effects")))
        {
            SetupBloom(cmd, cameraColorTarget);

            m_composite.SetFloat("_Cutoff", m_effect.dotsCutoff.value);
            m_composite.SetFloat("_Density", m_effect.dotsDensity.value);
            m_composite.SetVector("_Direction", m_effect.scrollDirection.value);
            m_composite.SetTexture("_Bloom_Texture", m_BloomMipUp[0]);

            Blitter.BlitCameraTexture(cmd, cameraColorTarget, cameraColorTarget, m_composite, 0);
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

    public void SetTarget(RTHandle camerColorTargetHandle, RTHandle camerDepthTargetHandle)
    {
        cameraColorTarget = camerColorTargetHandle;
        cameraDepthTarget = camerDepthTargetHandle;
    }


    void SetupBloom(CommandBuffer cmd, RTHandle source)
    {
        // Start at half—res
        int downres = 1;
        int tw = m_Descriptor.width >> downres;
        int th = m_Descriptor.height >> downres;

        // Determine the iteration count
        int maxSize = Mathf.Max(tw, th);
        int iterations = Mathf.FloorToInt(Mathf.Log(maxSize, 26));
        int mipCount = Mathf.Clamp(iterations, 1, m_effect.maxIterations.value);

        // Pre—filtering parameters
        float clamp = m_effect.clamp.value;
        float threshold = Mathf.GammaToLinearSpace(m_effect.threshold.value);
        float thresholdKnee = threshold * 0.5f; // Hardcoded soft knee

        // Material setup
        float scatter = Mathf.Lerp(0.05f, 0.95f, m_effect.scatter.value);
        var bloomMaterial = m_render;

        bloomMaterial.SetVector("_Params", new Vector4(scatter, clamp, threshold, thresholdKnee));

        // Prefilter
        var desc = GetCompatibleDescriptor(tw, th, hdrFormat);

        for (int i = 0; i < mipCount; i++)
        {
            RenderingUtils.ReAllocateIfNeeded(ref m_BloomMipUp[i], desc, FilterMode.Point, TextureWrapMode.Clamp,
                name: m_BloomMipUp[i].name);
            RenderingUtils.ReAllocateIfNeeded(ref m_BloomMipDown[i], desc, FilterMode.Point, TextureWrapMode.Clamp,
                name: m_BloomMipDown[i].name);
            desc.width = Mathf.Max(1, desc.width >> 1);
            desc.height = Mathf.Max(1, desc.height >> 1);
        }

        Blitter.BlitCameraTexture(cmd, source, m_BloomMipDown[0], RenderBufferLoadAction.DontCare,
            RenderBufferStoreAction.Store, m_render, 0);


        var lastDown = m_BloomMipDown[0];
        for (int i = 0; i < mipCount; i++)
        {
            //Classic two pass gaussian blur — use mipUp as a temporary target
            //First pass does 2x downsampling +9—tap gaussian
            //Second pass does 9—tap gaussian using a 5—tap filter +bilinear filtering
            Blitter.BlitCameraTexture(cmd, lastDown, m_BloomMipUp[i], RenderBufferLoadAction.DontCare,
                RenderBufferStoreAction.Store, bloomMaterial, 1);
            Blitter.BlitCameraTexture(cmd, m_BloomMipUp[i], m_BloomMipDown[i], RenderBufferLoadAction.DontCare,
                RenderBufferStoreAction.Store, bloomMaterial, 2);

            lastDown = m_BloomMipUp[i];
        }

        // Upsample (bilinear by default, HQ filtering does bicubic instead
        for (int i = mipCount - 2; i >= 0; i--)
        {
            var lowMip = (i == mipCount - 2) ? m_BloomMipDown[i + 1] : m_BloomMipDown[i + 1];
            var highMip = m_BloomMipDown[i];
            var dst = m_BloomMipUp[i];

            cmd.SetGlobalTexture("_SourceTexLowMip", lowMip);
            Blitter.BlitCameraTexture(cmd, highMip, dst, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 3);

        }

        cmd.SetGlobalTexture("_Bloom_Texture", m_BloomMipUp[0]);
        cmd.SetGlobalFloat("_BloomIntensity", m_effect.intensity.value);
    }
}

