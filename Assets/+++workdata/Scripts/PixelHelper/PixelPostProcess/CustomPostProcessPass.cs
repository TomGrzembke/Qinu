using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class CustomPostProcessPass : ScriptableRenderPass
{
    RenderTextureDescriptor descriptor;
    RTHandle cameraColorTarget;
    RTHandle cameraDepthTarget;

    Material m_render;
    Material m_composite;

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
        m_render = stack.GetComponent<PixelPostProcessComponent>();

        CommandBuffer cmd = CommandBufferPool.Get();

        using(new ProfilingScope(cmd, new ProfilingSampler("Custom Post Process Effects")))
        {
            SetupBloom(cmd, cameraColorTarget); //stopped at https://youtu.be/9fa4uFm1eCE?si=bPLv4EnMKUwySzWd&t=818
        }
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        descriptor = renderingData.cameraData.cameraTargetDescriptor;
    }

    public void SetTarget(RTHandle camerColorTargetHandle, RTHandle camerDepthTargetHandle)
    {
        cameraColorTarget = camerColorTargetHandle;
        cameraDepthTarget = camerDepthTargetHandle;
    }
}
