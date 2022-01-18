using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricLightComposePass : ScriptableRenderPass {

    const string profilerTag = "Volumetric Light Compose";
    ProfilingSampler profilingSampler = new ProfilingSampler (profilerTag);

    RenderTargetHandle volumetricLightTexture;

    public VolumetricLightComposePass (RenderPassEvent renderPassEvent, RenderTargetHandle volumetricLightTexture) {
        this.renderPassEvent = renderPassEvent;
        this.volumetricLightTexture = volumetricLightTexture;
    }

    public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData) {
        var cmd = CommandBufferPool.Get (profilerTag);
        Blit (cmd, volumetricLightTexture.id, BuiltinRenderTextureType.CurrentActive);
        context.ExecuteCommandBuffer (cmd);
        CommandBufferPool.Release (cmd);
    }
}
