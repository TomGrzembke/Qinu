using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostProcessRenderFeature : ScriptableRendererFeature
{
    [SerializeField] Shader bloomShader;
    [SerializeField] Shader compositeShader;
    [SerializeField] Shader defComShader;

    Material m_bloom;
    Material compositeMaterial;
    Material m_defCom;


    CustomPostProcessPass customPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(customPass);
        }
    }
    public override void Create()
    {
        m_bloom = CoreUtils.CreateEngineMaterial(bloomShader);
        compositeMaterial = CoreUtils.CreateEngineMaterial(compositeShader);
        m_defCom = CoreUtils.CreateEngineMaterial(defComShader);

        customPass = new CustomPostProcessPass(m_bloom, compositeMaterial, m_defCom);
    }
    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_bloom);
        CoreUtils.Destroy(compositeMaterial);
    }
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            customPass.ConfigureInput(ScriptableRenderPassInput.Depth);
            customPass.ConfigureInput(ScriptableRenderPassInput.Color);
            customPass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
        }
    }
}
