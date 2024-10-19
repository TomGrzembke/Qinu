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

        using (new ProfilingScope(cmd, new ProfilingSampler("Pixelate")))
        {
            SetupPixel();

            CalculatePixelScaling();
            m_composite.SetVector("_Resolution", scaling);
            m_composite.SetFloat("_OutlineThickness", m_effect.lineSize.value);
            m_composite.SetColor("_OutlineCol", m_effect.lineCol.value);
            m_composite.SetTexture("_MainTex", cameraColorTarget);

            Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_MainTex, m_composite, 0);
            m_DefCom.SetTexture("_OriginalTex", m_MainTex);
            m_composite.SetTexture("_OriginalTex", m_MainTex);
            Blitter.BlitCameraTexture(cmd, m_MainTex, cameraColorTarget, m_DefCom, 0);
            //Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_MainTex, m_composite, 0);
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

    void SetupPixel()
    {
        var targetWidth = m_Descriptor.width;
        var targetHeight = m_Descriptor.height;

        var desc = GetCompatibleDescriptor(targetWidth, targetHeight, hdrFormat);

        RenderingUtils.ReAllocateIfNeeded(ref m_MainTex, desc, FilterMode.Point, TextureWrapMode.Clamp, name: m_MainTex.name);
    }

    void CalculatePixelScaling()
    {
        float pixelResWidth = m_effect.pixelRes.value;

        float targetWidth = m_Descriptor.width;
        float targetHeight = m_Descriptor.height;

        screenRatio = new(1.777543f, 1);
        //float greatestCommonFactor = CalculateGCF(screenRatio);


        scaling.x = pixelResWidth;
        scaling.y = (targetHeight / targetWidth) * pixelResWidth;
        //scaling.y = scaling.y;

        //Debug.Log(greatestCommonFactor);
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