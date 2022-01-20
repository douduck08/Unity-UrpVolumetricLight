using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricLightComposePass : ScriptableRenderPass {

    const string profilerTag = "Volumetric Light Compose";
    ProfilingSampler profilingSampler = new ProfilingSampler (profilerTag);

    Material volumetricLightMaterial;
    VolumetricLight.Settings settings;
    RenderTargetHandle volumetricLightTexture;
    RenderTargetHandle cameraColorTexture;
    RenderTargetHandle tempTexture;

    public VolumetricLightComposePass (RenderPassEvent renderPassEvent, Material volumetricLightMaterial) {
        this.renderPassEvent = renderPassEvent;
        this.volumetricLightMaterial = volumetricLightMaterial;
        volumetricLightTexture.Init (VolumetricLight.ShaderIds.volumetricLightTexture);
        cameraColorTexture.Init (VolumetricLight.ShaderIds.cameraColorTexture);
        tempTexture.Init (VolumetricLight.ShaderIds.tempTexture);
    }

    public void Setup (VolumetricLight.Settings settings) {
        this.settings = settings;
    }

    public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData) {
        var cmd = CommandBufferPool.Get (profilerTag);

        if (settings.blurSteps == 0) {
            cmd.Blit (BuiltinRenderTextureType.None, cameraColorTexture.id, volumetricLightMaterial, 1);
        } else if (settings.blurSteps == 1) {
            cmd.Blit (BuiltinRenderTextureType.None, cameraColorTexture.id, volumetricLightMaterial, 2);
        } else if (settings.blurSteps == 2) {
            cmd.Blit (BuiltinRenderTextureType.None, tempTexture.id, volumetricLightMaterial, 3);
            cmd.SetGlobalTexture (volumetricLightTexture.id, tempTexture.id);
            cmd.Blit (BuiltinRenderTextureType.None, cameraColorTexture.id, volumetricLightMaterial, 2);
        }

        context.ExecuteCommandBuffer (cmd);
        CommandBufferPool.Release (cmd);
    }

    public override void Configure (CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
        if (settings.blurSteps == 2) {
            var descriptor = cameraTextureDescriptor;
            descriptor.useMipMap = false;
            cmd.GetTemporaryRT (tempTexture.id, descriptor);
        }
        ConfigureTarget (cameraColorTexture.id);
    }

    public override void FrameCleanup (CommandBuffer cmd) {
        if (cmd == null) {
            throw new System.ArgumentNullException ("VolumetricLightPass.FrameCleanup(): cmd is null");
        }
        if (settings.blurSteps == 2) {
            cmd.ReleaseTemporaryRT (tempTexture.id);
        }
    }
}
