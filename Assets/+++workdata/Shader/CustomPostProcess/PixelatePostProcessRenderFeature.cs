using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelatePostProcessRenderFeature : ScriptableRendererFeature
{
    [SerializeField] Shader pixelShader;
    [SerializeField] Shader compositeShader;
    [SerializeField] Shader defComShader;

    Material m_defCom;
    Material m_Pixelate;
    Material m_Composite;

    PixelatePostProcessPass pixelPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(pixelPass);
        }
    }

    public override void Create()
    {
        m_Pixelate = CoreUtils.CreateEngineMaterial(pixelShader);
        m_Composite = CoreUtils.CreateEngineMaterial(compositeShader);
        m_defCom = CoreUtils.CreateEngineMaterial(defComShader);

        pixelPass = new PixelatePostProcessPass(m_Pixelate, m_Composite, m_defCom);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_Pixelate);
        CoreUtils.Destroy(m_Composite);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            pixelPass.ConfigureInput(ScriptableRenderPassInput.Depth);
            pixelPass.ConfigureInput(ScriptableRenderPassInput.Color);
            pixelPass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
        }
    }
}