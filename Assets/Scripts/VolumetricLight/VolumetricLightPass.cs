using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricLightPass : ScriptableRenderPass {

    const string profilerTag = "Volumetric Light";
    ProfilingSampler profilingSampler = new ProfilingSampler (profilerTag);

    ComputeShader volumetricLightCS;
    RenderTargetHandle volumetricLightTexture;
    RenderTargetHandle cameraDepthTexture;
    RenderTargetHandle mainShadowmap;
    Vector3[] frustumCorners = new Vector3[4];
    Matrix4x4 frustumCornersMatrix;
    Vector4 zBufferParam;

    public VolumetricLightPass (RenderPassEvent renderPassEvent, ComputeShader volumetricLightCS, RenderTargetHandle volumetricLightTexture) {
        this.renderPassEvent = renderPassEvent;
        this.volumetricLightCS = volumetricLightCS;
        this.volumetricLightTexture = volumetricLightTexture;

        cameraDepthTexture.Init (ShaderIds.cameraDepthTexture);
        mainShadowmap.Init (ShaderIds.mainShadowmap);
    }

    public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData) {
        ref var cameraData = ref renderingData.cameraData;
        var descriptor = cameraData.cameraTargetDescriptor;
        descriptor.enableRandomWrite = true;

        var cmd = CommandBufferPool.Get (profilerTag);
        cmd.SetComputeTextureParam (volumetricLightCS, 0, cameraDepthTexture.id, cameraDepthTexture.id);
        cmd.SetComputeTextureParam (volumetricLightCS, 0, mainShadowmap.id, mainShadowmap.id);
        cmd.SetComputeTextureParam (volumetricLightCS, 0, volumetricLightTexture.id, volumetricLightTexture.id);
        SetCameraParameters (cmd, volumetricLightCS, cameraData.camera);

        cmd.DispatchCompute (volumetricLightCS, 0, descriptor.width, descriptor.height, 1);

        context.ExecuteCommandBuffer (cmd);
        CommandBufferPool.Release (cmd);
    }

    public override void Configure (CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
        var descriptor = cameraTextureDescriptor;
        descriptor.enableRandomWrite = true;
        cmd.GetTemporaryRT (volumetricLightTexture.id, descriptor);
    }

    public override void FrameCleanup (CommandBuffer cmd) {
        if (cmd == null) {
            throw new System.ArgumentNullException ("VolumetricLightPass.FrameCleanup(): cmd is null");
        }
        cmd.ReleaseTemporaryRT (volumetricLightTexture.id);
    }

    void SetCameraParameters (CommandBuffer cmd, ComputeShader volumetricLightCS, Camera camera) {
        var transform = camera.transform;
        var cameraPosition = transform.position;
        camera.CalculateFrustumCorners (new Rect (0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        frustumCornersMatrix.SetRow (0, transform.localToWorldMatrix.MultiplyVector (frustumCorners[0]));
        frustumCornersMatrix.SetRow (1, transform.localToWorldMatrix.MultiplyVector (frustumCorners[1]));
        frustumCornersMatrix.SetRow (2, transform.localToWorldMatrix.MultiplyVector (frustumCorners[2]));
        frustumCornersMatrix.SetRow (3, transform.localToWorldMatrix.MultiplyVector (frustumCorners[3]));

        var f = camera.farClipPlane;
        var n = camera.nearClipPlane;
        zBufferParam.x = (f - n) / n;
        zBufferParam.y = 1.0f;
        zBufferParam.z = zBufferParam.x / f;
        zBufferParam.w = zBufferParam.y / f;

        cmd.SetComputeVectorParam (volumetricLightCS, "_ZBufferParam", zBufferParam);
        cmd.SetComputeVectorParam (volumetricLightCS, "_CameraPosition", cameraPosition);
        cmd.SetComputeMatrixParam (volumetricLightCS, "_FrustumCorners", frustumCornersMatrix);
    }
}
