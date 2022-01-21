using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricLightPass : ScriptableRenderPass {


    const string profilerTag = "Volumetric Light";
    ProfilingSampler profilingSampler = new ProfilingSampler (profilerTag);

    Material volumetricLightMaterial;
    VolumetricLight.Settings settings;
    RenderTargetHandle volumetricLightTexture;

    Vector3[] frustumCorners = new Vector3[4];
    Matrix4x4 frustumCornersMatrix;
    Vector4 zBufferParam;
    Vector4 texelSize;

    public VolumetricLightPass (RenderPassEvent renderPassEvent, Material volumetricLightMaterial) {
        this.renderPassEvent = renderPassEvent;
        this.volumetricLightMaterial = volumetricLightMaterial;
        volumetricLightTexture.Init (VolumetricLight.ShaderIds.volumetricLightTexture);
    }

    public void Setup (VolumetricLight.Settings settings) {
        this.settings = settings;
    }

    public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData) {
        ref var cameraData = ref renderingData.cameraData;
        SetCameraParameters (cameraData.camera);
        SetMaterialParameters ();

        var cmd = CommandBufferPool.Get (profilerTag);
        cmd.Blit (BuiltinRenderTextureType.None, volumetricLightTexture.id, volumetricLightMaterial, 0);
        context.ExecuteCommandBuffer (cmd);
        CommandBufferPool.Release (cmd);
    }

    public override void Configure (CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
        var descriptor = cameraTextureDescriptor;
        descriptor.useMipMap = false;
        if (settings.downsample) {
            descriptor.width = descriptor.width >> 1;
            descriptor.height = descriptor.height >> 1;
        }
        cmd.GetTemporaryRT (volumetricLightTexture.id, descriptor, FilterMode.Bilinear);
        ConfigureTarget (volumetricLightTexture.id);

        texelSize = new Vector4 (1f / descriptor.width, 1f / descriptor.height, descriptor.width, descriptor.height);
    }

    public override void FrameCleanup (CommandBuffer cmd) {
        if (cmd == null) {
            throw new System.ArgumentNullException ("VolumetricLightPass.FrameCleanup(): cmd is null");
        }
        cmd.ReleaseTemporaryRT (volumetricLightTexture.id);
    }

    void SetCameraParameters (Camera camera) {
        var transform = camera.transform;
        camera.CalculateFrustumCorners (new Rect (0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        frustumCornersMatrix.SetRow (0, transform.localToWorldMatrix.MultiplyVector (frustumCorners[0]));
        frustumCornersMatrix.SetRow (1, transform.localToWorldMatrix.MultiplyVector (frustumCorners[1]));
        frustumCornersMatrix.SetRow (2, transform.localToWorldMatrix.MultiplyVector (frustumCorners[2]));
        frustumCornersMatrix.SetRow (3, transform.localToWorldMatrix.MultiplyVector (frustumCorners[3]));
        volumetricLightMaterial.SetMatrix ("_FrustumCorners", frustumCornersMatrix);
    }

    void SetMaterialParameters () {
        volumetricLightMaterial.SetInt ("_SampleNumber", settings.sampleNumber);
        volumetricLightMaterial.SetFloat ("_RandomStrength", settings.randomStrength);
        volumetricLightMaterial.SetFloat ("_Intensity", settings.intensity);
        volumetricLightMaterial.SetFloat ("_Scattering", settings.scattering);
        volumetricLightMaterial.SetVector ("_TexelSize", texelSize);
    }
}
