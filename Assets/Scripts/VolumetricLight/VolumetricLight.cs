using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class VolumetricLight : ScriptableRendererFeature {

    public static class ShaderIds {
        public readonly static string cameraColorTexture = "_CameraColorTexture";
        public readonly static string volumetricLightTexture = "_VolumetricLightTexture";
        public readonly static string tempTexture = "_VolumetricLightTemp";
    }

    [SerializeField] Shader volumetricLightShader;
    [SerializeField] Texture[] blueNoiseTextures;
    int noiseIndex;

    Material volumetricLightMaterial;
    VolumetricLightPass volumetricLightPass;
    VolumetricLightComposePass volumetricLightComposePass;

    [System.Serializable]
    public struct Settings {
        [Range (2, 128)] public int sampleNumber;
        [Range (0, 2)] public int downsample;
        [Range (0, 2)] public int blurSteps;
        [Range (0f, 0.99f)] public float scattering;
        [Range (0f, 1f)] public float randomStrength;
    }
    public Settings settings = new Settings { sampleNumber = 4, downsample = 0, scattering = 0.5f };

    public override void Create () {
        if (volumetricLightMaterial == null) {
            volumetricLightMaterial = new Material (volumetricLightShader);
        }
        volumetricLightPass = new VolumetricLightPass (RenderPassEvent.AfterRenderingPrePasses, volumetricLightMaterial);
        volumetricLightComposePass = new VolumetricLightComposePass (RenderPassEvent.BeforeRenderingPostProcessing, volumetricLightMaterial);
    }

    public override void AddRenderPasses (ScriptableRenderer renderer, ref RenderingData renderingData) {
        SetupNoise ();
        volumetricLightPass.Setup (settings);
        volumetricLightComposePass.Setup (settings);
        renderer.EnqueuePass (volumetricLightPass);
        renderer.EnqueuePass (volumetricLightComposePass);
    }

    void SetupNoise () {
        if (blueNoiseTextures != null && blueNoiseTextures.Length > 0) {
            noiseIndex = (noiseIndex + 1) % blueNoiseTextures.Length;
            volumetricLightMaterial.SetTexture ("_BlueNoise", blueNoiseTextures[noiseIndex]);
        }
    }
}
