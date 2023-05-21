using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DeepDreams.RenderFeatures
{
    public class GaussianBlurRenderPass : ScriptableRenderPass, IDisposable
    {
        [Serializable]
        public class BlurSettings
        {
            [Range(2, 100)] public int iterations = 51;
            [Range(0, 0.5f)] public float blurSize = 0.25f;
            [Range(0, 0.3f)] public float standardDeviation = 0.02f;
        }

        private enum ShaderPass
        {
            Copy = 0,
            VerticalBlur = 1,
            HorizontalBlur = 2
        }

        // Settings
        private readonly string _profilerTag;
        private readonly BlurSettings _blurSettings;
        private readonly CustomBlurRenderFeature.Settings _featureSettings;

        // Resources
        private readonly Material _material;
        private readonly Texture2D _blueNoiseTexture;

        // Render Targets
        private RenderTargetIdentifier _colorTarget, _tempTarget, _tempTarget2;
        private RenderTextureDescriptor _cameraDescriptor;

        // Shader property IDs
        private readonly int TempTargetID = Shader.PropertyToID("_TempTarget");
        private readonly int TempTarget2ID = Shader.PropertyToID("_TempTarget2");
        private readonly int IterationsID = Shader.PropertyToID("_Iterations");
        private readonly int BlurSizeID = Shader.PropertyToID("_BlurSize");
        private readonly int StandardDeviationID = Shader.PropertyToID("_StandardDeviation");
        private readonly int BlueNoiseProperty = Shader.PropertyToID("_BlueNoise");
        private readonly int DitheringParamsProperty = Shader.PropertyToID("_Dithering_Params");

        // The constructor of the pass. Here you can set any material properties that do not need to be updated on a per-frame basis.
        public GaussianBlurRenderPass(string profilerTag, RenderPassEvent renderEvent, BlurSettings blurSettings,
            CustomBlurRenderFeature.Settings featureSettings)
        {
            _profilerTag = profilerTag;
            renderPassEvent = renderEvent;

            _blurSettings = blurSettings;
            _featureSettings = featureSettings;

            if (_material == null)
            {
                _material = CoreUtils.CreateEngineMaterial("Hidden/Blur/GaussianBlur");
                _blueNoiseTexture = Resources.Load<Texture2D>("Blue Noise/LDR_RGBA_0");

                if (_blueNoiseTexture != null)
                {
                    _material.SetTexture(BlueNoiseProperty, _blueNoiseTexture);
                }
            }
        }

        // Called per-pass
        // Same as OnCameraSetup() below.
        // public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        // {}

        // Called per-camera.
        // Gets called by the renderer before executing the pass.
        // Can be used to configure render targets and their clearing state.
        // Can be used to create temporary render target textures.
        // If this method is not overriden, the render pass will render to the active camera render target.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // Grab the camera target descriptor. We will use this when creating a temporary render texture.
            _cameraDescriptor = renderingData.cameraData.cameraTargetDescriptor;

            // Set the number of depth bits we need for our temporary render texture.
            _cameraDescriptor.depthBufferBits = 0;

            // Create a temporary render texture using the descriptor from above.
            cmd.GetTemporaryRT(TempTargetID, _cameraDescriptor, FilterMode.Bilinear);
            cmd.GetTemporaryRT(TempTarget2ID, _cameraDescriptor, FilterMode.Bilinear);
            _tempTarget = new RenderTargetIdentifier(TempTargetID);
            _tempTarget2 = new RenderTargetIdentifier(TempTarget2ID);

            // Grab the color buffer from the renderer camera color target.
            _colorTarget = renderingData.cameraData.renderer.cameraColorTarget;

            _material.SetInteger(IterationsID, _blurSettings.iterations);
            _material.SetFloat(BlurSizeID, _blurSettings.blurSize);
            _material.SetFloat(StandardDeviationID, _blurSettings.standardDeviation);

            if (_featureSettings.dithering)
            {
                _material.EnableKeyword("ENABLE_DITHERING");

                _material.SetVector(DitheringParamsProperty, new Vector4(
                    renderingData.cameraData.camera.scaledPixelWidth / (float)_blueNoiseTexture.width,
                    renderingData.cameraData.camera.scaledPixelHeight / (float)_blueNoiseTexture.height,
                    1,
                    1
                ));
            }
            else
            {
                _material.DisableKeyword("ENABLE_DITHERING");
            }

            // Who knows what the point of this is when it invalidates the state changes made from the first ConfigureTarget() call.
            // https://bronsonzgeb.com/index.php/2021/03/20/pseudo-metaballs-with-scriptable-renderer-features-in-unitys-urp/
            ConfigureTarget(new[] { _tempTarget, _tempTarget2 });
        }

        public void SetTarget(RenderTargetIdentifier camerColorTargetIdentifier)
        {
            _colorTarget = camerColorTargetIdentifier;
        }

        // The actual execution of the pass. This is where custom rendering occurs.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler(_profilerTag)))
            {
                // Blit from the color buffer to a temporary buffer and back. This is needed for a two-pass shader.
                Blit(cmd, _colorTarget, _tempTarget, _material);

                if (_featureSettings.copyToCameraFramebuffer)
                {
                    Blit(cmd, _tempTarget, _colorTarget, _material, 1);
                }
                else
                {
                    Blit(cmd, _tempTarget, _tempTarget2, _material, 1);
                    cmd.SetGlobalTexture(_featureSettings.blurTextureName, _tempTarget2);
                }
            }

            // Execute the command buffer and release it.
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        // Called when the camera has finished rendering.
        // Here we release/cleanup any allocated resources that were created by this pass.
        // Gets called for all cameras in a camera stack.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException(nameof(cmd));
            }

            // Since we created a temporary render texture in OnCameraSetup, we need to release the memory here to avoid a leak.
            cmd.ReleaseTemporaryRT(TempTargetID);
            cmd.ReleaseTemporaryRT(TempTarget2ID);
        }

        public void Dispose()
        {
            CoreUtils.Destroy(_material);
        }
    }
}