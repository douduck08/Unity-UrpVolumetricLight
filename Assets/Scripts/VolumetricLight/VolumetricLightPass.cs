using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricLightPass : ScriptableRenderPass {

    public static class ShaderIds {
        public readonly static string volumetricLightTexture = "_VolumetricLightTexture";
        public readonly static string cameraDepthTexture = "_CameraDepthTexture";
        public readonly static string mainShadowmap = "_MainLightShadowmapTexture";
    }

    const string profilerTag = "Volumetric Light";
    ProfilingSampler profilingSampler = new ProfilingSampler (profilerTag);

    Material volumetricLightMaterial;
    RenderTargetHandle volumetricLightTexture;
    // RenderTargetHandle cameraDepthTexture;
    // RenderTargetHandle mainShadowmap;
    Vector3[] frustumCorners = new Vector3[4];
    Matrix4x4 frustumCornersMatrix;
    Vector4 zBufferParam;

    public VolumetricLightPass (RenderPassEvent renderPassEvent, Material volumetricLightMaterial) {
        this.renderPassEvent = renderPassEvent;
        this.volumetricLightMaterial = volumetricLightMaterial;

        volumetricLightTexture.Init (ShaderIds.volumetricLightTexture);
        // cameraDepthTexture.Init (ShaderIds.cameraDepthTexture);
        // mainShadowmap.Init (ShaderIds.mainShadowmap);
    }

    public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData) {
        ref var cameraData = ref renderingData.cameraData;
        SetCameraParameters (volumetricLightMaterial, cameraData.camera);

        var cmd = CommandBufferPool.Get (profilerTag);
        cmd.Blit (BuiltinRenderTextureType.None, volumetricLightTexture.id, volumetricLightMaterial, 0);
        context.ExecuteCommandBuffer (cmd);
        CommandBufferPool.Release (cmd);
    }

    public override void Configure (CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
        var descriptor = cameraTextureDescriptor;
        cmd.GetTemporaryRT (volumetricLightTexture.id, descriptor);
        ConfigureTarget (volumetricLightTexture.id);
    }

    public override void FrameCleanup (CommandBuffer cmd) {
        if (cmd == null) {
            throw new System.ArgumentNullException ("VolumetricLightPass.FrameCleanup(): cmd is null");
        }
        cmd.ReleaseTemporaryRT (volumetricLightTexture.id);
    }

    void SetCameraParameters (Material material, Camera camera) {
        var transform = camera.transform;
        camera.CalculateFrustumCorners (new Rect (0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        frustumCornersMatrix.SetRow (0, transform.localToWorldMatrix.MultiplyVector (frustumCorners[0]));
        frustumCornersMatrix.SetRow (1, transform.localToWorldMatrix.MultiplyVector (frustumCorners[1]));
        frustumCornersMatrix.SetRow (2, transform.localToWorldMatrix.MultiplyVector (frustumCorners[2]));
        frustumCornersMatrix.SetRow (3, transform.localToWorldMatrix.MultiplyVector (frustumCorners[3]));
        material.SetMatrix ("_FrustumCorners", frustumCornersMatrix);
    }
}
