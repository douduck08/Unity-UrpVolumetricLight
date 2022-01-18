using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class VolumetricLight : ScriptableRendererFeature {

    [SerializeField] ComputeShader volumetricLightCS;
    RenderTargetHandle volumetricLightTexture;

    VolumetricLightPass volumetricLightPass;
    VolumetricLightComposePass volumetricLightComposePass;

    public override void Create () {
        volumetricLightTexture.Init (ShaderIds.volumetricLightTexture);
        volumetricLightPass = new VolumetricLightPass (RenderPassEvent.AfterRenderingOpaques, volumetricLightCS, volumetricLightTexture);
        volumetricLightComposePass = new VolumetricLightComposePass (RenderPassEvent.BeforeRenderingPostProcessing, volumetricLightTexture);
    }

    public override void AddRenderPasses (ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass (volumetricLightPass);
        renderer.EnqueuePass (volumetricLightComposePass);
    }
}
