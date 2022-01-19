using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricLightComposePass : ScriptableRenderPass {

    const string profilerTag = "Volumetric Light Compose";
    ProfilingSampler profilingSampler = new ProfilingSampler (profilerTag);

    Material volumetricLightMaterial;

    public VolumetricLightComposePass (RenderPassEvent renderPassEvent, Material volumetricLightMaterial) {
        this.renderPassEvent = renderPassEvent;
        this.volumetricLightMaterial = volumetricLightMaterial;
    }

    public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData) {
        var cmd = CommandBufferPool.Get (profilerTag);
        cmd.Blit (BuiltinRenderTextureType.None, BuiltinRenderTextureType.CurrentActive, volumetricLightMaterial, 1);
        context.ExecuteCommandBuffer (cmd);
        CommandBufferPool.Release (cmd);
    }
}
