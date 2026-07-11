using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BenDayPostProcessRenderFeature : ScriptableRendererFeature
{
    [SerializeField] Shader bloomShader;
    [SerializeField] Shader compositeShader;
    [SerializeField] Shader defComShader;

    Material m_bloom;
    Material compositeMaterial;
    Material m_defCom;

    BenDayPostProcessPass customPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            customPass.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Color);
            renderer.EnqueuePass(customPass);
        }
    }
    public override void Create()
    {
        m_bloom = CoreUtils.CreateEngineMaterial(bloomShader);
        compositeMaterial = CoreUtils.CreateEngineMaterial(compositeShader);
        m_defCom = CoreUtils.CreateEngineMaterial(defComShader);

        customPass = new BenDayPostProcessPass(m_bloom, compositeMaterial, m_defCom);
    }
    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_bloom);
        CoreUtils.Destroy(m_defCom);
        CoreUtils.Destroy(compositeMaterial);
    }
   
}
