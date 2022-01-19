using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class VolumetricLight : ScriptableRendererFeature {

    [SerializeField] Shader volumetricLightShader;

    Material volumetricLightMaterial;
    VolumetricLightPass volumetricLightPass;
    VolumetricLightComposePass volumetricLightComposePass;

    public override void Create () {
        if (volumetricLightMaterial == null) {
            volumetricLightMaterial = new Material (volumetricLightShader);
        }
        volumetricLightPass = new VolumetricLightPass (RenderPassEvent.AfterRenderingPrePasses, volumetricLightMaterial);
        volumetricLightComposePass = new VolumetricLightComposePass (RenderPassEvent.BeforeRenderingPostProcessing, volumetricLightMaterial);
    }

    public override void AddRenderPasses (ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass (volumetricLightPass);
        renderer.EnqueuePass (volumetricLightComposePass);
    }
}
