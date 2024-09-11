using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostProcessRenderFeature : ScriptableRendererFeature
{
    [SerializeField] Shader pixelShader;
    [SerializeField] Shader compositeShader;

    Material pixelMaterial;
    Material compositeMaterial;


    CustomPostProcessPass customPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(customPass);
    }
    public override void Create()
    {
        pixelMaterial = CoreUtils.CreateEngineMaterial(pixelShader);
        compositeMaterial = CoreUtils.CreateEngineMaterial(compositeShader);

        customPass = new CustomPostProcessPass(pixelMaterial, compositeMaterial);
    }
    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(pixelMaterial);
        CoreUtils.Destroy(compositeMaterial);
    }
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            customPass.ConfigureInput(ScriptableRenderPassInput.Depth);
            customPass.ConfigureInput(ScriptableRenderPassInput.Color);
            customPass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraColorTargetHandle);

        }
    }
}
