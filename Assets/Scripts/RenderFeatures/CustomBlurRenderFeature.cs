using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DeepDreams.RenderFeatures
{
    public class CustomBlurRenderFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class Settings
        {
            [Tooltip("Send the buffer texture to the screen.")]
            public bool copyToCameraFramebuffer;
            [Tooltip("Apply the blur effect to the scene view camera.")]
            public bool showInSceneView;
            [Tooltip("Apply dithering to the final output to reduce color banding.")]
            public bool dithering;
            [Tooltip("Perform blurring operations on HDR buffers.")]
            public bool hdrFiltering;
            [Tooltip("The name of the global texture to store the final blurred image into.")]
            public string blurTextureName = "_BlurTexture";
            public BlurModes blurModes;
        }
        public enum BlurModes
        {
            Gaussian,
            // Kawase,
            KawaseDualFilter
        }

        // Where/when the render pass should be injected during the rendering process.
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        public Settings settings = new Settings();

        public GaussianBlurRenderPass.BlurSettings gaussianSettings = new GaussianBlurRenderPass.BlurSettings();
        public KawaseDualFilterBlurRenderPass.BlurSettings kawaseDualFilterSettings =
            new KawaseDualFilterBlurRenderPass.BlurSettings();

        private ScriptableRenderPass _activePass;

        // Gets called every time serialization happens.
        // Gets called when you enable/disable the renderer feature.
        // Gets called when you change a property in the inspector of the renderer feature.
        public override void Create()
        {
            name = "Custom Blur";

            switch (settings.blurModes)
            {
                case BlurModes.Gaussian:
                    GaussianBlurRenderPass _gaussianPass = new GaussianBlurRenderPass("Gaussian Blur", renderPassEvent,
                        gaussianSettings, settings);
                    _activePass = _gaussianPass;
                    break;
                case BlurModes.KawaseDualFilter:
                    KawaseDualFilterBlurRenderPass _kawaseDualFilterPass = new KawaseDualFilterBlurRenderPass(
                        "Kawase Dual-Filter Blur", renderPassEvent,
                        kawaseDualFilterSettings, settings);
                    _activePass = _kawaseDualFilterPass;
                    break;
                // case BlurModes.Kawase:
                // break;
            }
        }

        // Injects one or multiple render passes in the renderer.
        // Gets called when setting up the renderer, once per-camera.
        // Gets called every frame, once per-camera.
        // Will not be called if the renderer feature is disabled in the renderer inspector.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Register our blur pass to the scriptable renderer.

            if (settings.copyToCameraFramebuffer && settings.showInSceneView)
            {
                _activePass.ConfigureInput(ScriptableRenderPassInput.Color);
                renderer.EnqueuePass(_activePass);
            }
            else if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                // https://forum.unity.com/threads/how-to-blit-in-urp-documentation-unity-blog-post-on-every-blit-function.1211508/#post-8375610
                // You can use ConfigureInput(Color); to make the opaque texture available in your scriptable render pass (regardless of what the renderer asset settings are).
                _activePass.ConfigureInput(ScriptableRenderPassInput.Color);
                // _activePass.SetTarget(renderer.cameraColorTarget);

                renderer.EnqueuePass(_activePass);
            }
        }
    }
}