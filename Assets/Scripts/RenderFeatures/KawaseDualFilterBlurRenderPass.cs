using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DeepDreams.RenderFeatures
{
    public class KawaseDualFilterBlurRenderPass : ScriptableRenderPass, IDisposable
    {
        [Serializable]
        public class BlurSettings
        {
            [Tooltip("Number of iterations of downsample->upsample passes. Higher = more blur.")]
            [Range(0, 6)] public int blurPasses = 2;
            [Tooltip("The size (strength) of the blur at each pass. Higher = more blur.")]
            [Range(0.0f, 10.0f)] public float blurSize = 1.0f;
        }

        private enum ShaderPass
        {
            Copy = 0,
            DownSample = 1,
            UpSample = 2,
            UpSampleDither = 3
        }

        // Settings
        private readonly string _profilerTag;
        private readonly BlurSettings _blurSettings;
        private readonly CustomBlurRenderFeature.Settings _featureSettings;

        // Resources
        private readonly Material _blurMaterial;
        private readonly Texture2D _blueNoiseTexture;

        // Render Targets
        private RenderTargetIdentifier _colorTarget, _startTarget, _endTarget;
        private RenderTextureDescriptor _cameraDescriptor, _targetsDescriptor;

        // Shader property IDs
        private readonly int EndTargetID = Shader.PropertyToID("_EndTarget");
        private readonly int StartTargetID = Shader.PropertyToID("_StartTarget");
        private readonly int BlendTexProperty = Shader.PropertyToID("_BlendTex");
        private readonly int BlueNoiseProperty = Shader.PropertyToID("_BlueNoise");
        private readonly int BlurSizeProperty = Shader.PropertyToID("_BlurSize");
        private readonly int DitheringParamsProperty = Shader.PropertyToID("_Dithering_Params");

        // The constructor of the pass. Here you can set any material properties that do not need to be updated on a per-frame basis.
        public KawaseDualFilterBlurRenderPass(string profilerTag, RenderPassEvent renderEvent,
            BlurSettings blurSettings, CustomBlurRenderFeature.Settings featureSettings)
        {
            _profilerTag = profilerTag;
            renderPassEvent = renderEvent;

            _blurSettings = blurSettings;
            _featureSettings = featureSettings;

            if (_blurMaterial == null)
            {
                _blurMaterial = CoreUtils.CreateEngineMaterial("Hidden/Blur/KawaseDualFilterBlur");
                _blueNoiseTexture = Resources.Load<Texture2D>("Blue Noise/LDR_RGBA_0");

                if (_blueNoiseTexture != null)
                {
                    _blurMaterial.SetTexture(BlueNoiseProperty, _blueNoiseTexture);
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

            // Start out at half res.
            _targetsDescriptor = _cameraDescriptor;
            _targetsDescriptor.width /= 2;
            _targetsDescriptor.height /= 2;
            _targetsDescriptor.useMipMap = false;

            // Helps prevent overblowing lights in the scene. That is useful for bloom but I don't want it here.
            // _targetsDescriptor.colorFormat = RenderTextureFormat.ARGBFloat;
            // _targetsDescriptor.graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat;

            // Create a temporary render texture using the descriptor from above.
            cmd.GetTemporaryRT(EndTargetID, _targetsDescriptor, FilterMode.Bilinear);
            cmd.GetTemporaryRT(StartTargetID, _targetsDescriptor, FilterMode.Bilinear);
            _endTarget = new RenderTargetIdentifier(EndTargetID);
            _startTarget = new RenderTargetIdentifier(StartTargetID);

            _blurMaterial.SetFloat(BlurSizeProperty, _blurSettings.blurSize);

            if (_featureSettings.dithering)
            {
                _blurMaterial.EnableKeyword("ENABLE_DITHERING");

                _blurMaterial.SetVector(DitheringParamsProperty, new Vector4(
                    renderingData.cameraData.camera.scaledPixelWidth / (float)_blueNoiseTexture.width,
                    renderingData.cameraData.camera.scaledPixelHeight / (float)_blueNoiseTexture.height,
                    1,
                    1
                ));
            }
            else
            {
                _blurMaterial.DisableKeyword("ENABLE_DITHERING");
            }

            // Grab the color buffer from the renderer camera color target.
            _colorTarget = renderingData.cameraData.renderer.cameraColorTarget;

            // Who knows what the point of this is when it invalidates the state changes made from the first ConfigureTarget() call.
            // https://bronsonzgeb.com/index.php/2021/03/20/pseudo-metaballs-with-scriptable-renderer-features-in-unitys-urp/
            ConfigureTarget(_endTarget);
            ConfigureTarget(_startTarget);
            // ConfigureClear(ClearFlag.All, Color.clear);
        }

        public void SetTarget(RenderTargetIdentifier camerColorTargetIdentifier)
        {
            _colorTarget = camerColorTargetIdentifier;
        }

        // The actual execution of the pass. This is where custom rendering occurs.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_blurSettings.blurPasses <= 0)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler(_profilerTag)))
            {
                // For Gaussian Blur
                // // Blit from the color buffer to a temporary buffer and back. This is needed for a two-pass shader.
                // Blit(cmd, _colorTarget, _temporaryBuffer, _material, 0);
                // // ReSharper disable once RedundantArgumentDefaultValue
                // Blit(cmd, _temporaryBuffer, _colorTarget, _material, 1);

                // First pass
                Blit(cmd, _colorTarget, _endTarget, _blurMaterial, (int)ShaderPass.DownSample);
                cmd.CopyTexture(_endTarget, _startTarget);

                cmd.ReleaseTemporaryRT(EndTargetID);

                // Downsample pass
                for (int i = 0; i < _blurSettings.blurPasses - 1; i++)
                {
                    _targetsDescriptor.width /= 2;
                    _targetsDescriptor.height /= 2;

                    cmd.GetTemporaryRT(EndTargetID, _targetsDescriptor, FilterMode.Bilinear);

                    Blit(cmd, _startTarget, _endTarget, _blurMaterial, (int)ShaderPass.DownSample);

                    cmd.ReleaseTemporaryRT(StartTargetID);
                    cmd.GetTemporaryRT(StartTargetID, _targetsDescriptor, FilterMode.Bilinear);
                    cmd.CopyTexture(_endTarget, _startTarget);

                    cmd.ReleaseTemporaryRT(EndTargetID);
                }

                // Upsample pass
                for (int i = _blurSettings.blurPasses - 1; i >= 0; i--)
                {
                    _targetsDescriptor.width = _cameraDescriptor.width / (int)Mathf.Pow(2, i);
                    _targetsDescriptor.height = _cameraDescriptor.height / (int)Mathf.Pow(2, i);

                    cmd.GetTemporaryRT(EndTargetID, _targetsDescriptor, FilterMode.Bilinear);

                    Blit(cmd, _startTarget, _endTarget, _blurMaterial, (int)ShaderPass.UpSample);

                    cmd.ReleaseTemporaryRT(StartTargetID);
                    cmd.GetTemporaryRT(StartTargetID, _targetsDescriptor, FilterMode.Bilinear);
                    cmd.CopyTexture(_endTarget, _startTarget);

                    cmd.ReleaseTemporaryRT(EndTargetID);
                }

                // Final pass
                if (_featureSettings.copyToCameraFramebuffer)
                {
                    Blit(cmd, _startTarget, _colorTarget, _blurMaterial);
                }
                else
                {
                    _targetsDescriptor.width = _cameraDescriptor.width;
                    _targetsDescriptor.height = _cameraDescriptor.height;

                    cmd.GetTemporaryRT(EndTargetID, _targetsDescriptor, FilterMode.Bilinear);
                    Blit(cmd, _startTarget, _endTarget, _blurMaterial);
                    cmd.SetGlobalTexture(_featureSettings.blurTextureName, _endTarget);
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
            cmd.ReleaseTemporaryRT(StartTargetID);
            cmd.ReleaseTemporaryRT(EndTargetID);
        }

        public void Dispose()
        {
            CoreUtils.Destroy(_blurMaterial);
        }
    }
}