using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
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
            [Tooltip("Use high quality bilinear sampling (more expensive but can help mitigate artifacts)")]
            public bool highQualityFiltering = true;
        }

        private enum ShaderPass
        {
            Copy = 0,
            DownSampleAntiFlicker = 1,
            DownSample = 2,
            UpSample = 3,
            FinalUpSample = 4
        }

        // Settings
        private readonly string _profilerTag;
        private readonly BlurSettings _blurSettings;
        private readonly CustomBlurRenderFeature.Settings _featureSettings;

        // Resources
        private readonly Material _material;
        private readonly Texture2D _blueNoiseTexture;

        // Render Targets
        private RenderTargetIdentifier _colorTarget;
        private readonly RenderTargetIdentifier[] _mipUpTarget;
        private readonly RenderTargetIdentifier[] _mipDownTarget;
        private RenderTextureDescriptor _cameraDescriptor, _targetsDescriptor;
        private readonly GraphicsFormat _hdrFormat;

        // Shader property IDs
        private readonly int[] _mipUpID;
        private readonly int[] _mipDownID;
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

            if (_material == null)
            {
                _material = CoreUtils.CreateEngineMaterial("Hidden/Blur/KawaseDualFilterBlur");
                _blueNoiseTexture = Resources.Load<Texture2D>("Blue Noise/LDR_RGBA_0");

                if (_blueNoiseTexture != null)
                {
                    _material.SetTexture(BlueNoiseProperty, _blueNoiseTexture);
                }
            }

            if (_blurSettings.highQualityFiltering)
            {
                _material.EnableKeyword("_HQ_FILTERING");
            }
            else
            {
                _material.DisableKeyword("_HQ_FILTERING");
            }

            _mipUpID = new int[_blurSettings.blurPasses];
            _mipDownID = new int[_blurSettings.blurPasses];
            _mipUpTarget = new RenderTargetIdentifier[_blurSettings.blurPasses];
            _mipDownTarget = new RenderTargetIdentifier[_blurSettings.blurPasses];

            for (int i = 0; i < _blurSettings.blurPasses; i++)
            {
                _mipUpID[i] = Shader.PropertyToID("_MipUp" + i);
                _mipDownID[i] = Shader.PropertyToID("_MipDown" + i);
                _mipUpTarget[i] = new RenderTargetIdentifier(_mipUpID[i]);
                _mipDownTarget[i] = new RenderTargetIdentifier(_mipDownID[i]);
            }

            const FormatUsage usage = FormatUsage.Linear | FormatUsage.Render;
            if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, usage) &&
                featureSettings.hdrFiltering) // HDR fallback
            {
                _hdrFormat = GraphicsFormat.B10G11R11_UFloatPack32;
            }
            else
            {
                _hdrFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
                    ? GraphicsFormat.R8G8B8A8_SRGB
                    : GraphicsFormat.R8G8B8A8_UNorm;
            }
        }

        // Called per-pass
        // Same as OnCameraSetup() below.
        // public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        // {
        // ConfigureTarget(_mipUpTarget);
        // ConfigureClear(ClearFlag.All, Color.black);
        // }

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
            _targetsDescriptor = _cameraDescriptor;
            _targetsDescriptor.depthBufferBits = 0;

            // Start out at half res.
            _targetsDescriptor.width >>= 1; // Bitwise right shift 1 = Divide by 2.
            _targetsDescriptor.height >>= 1;
            _targetsDescriptor.useMipMap = false;
            _targetsDescriptor.graphicsFormat = _hdrFormat;

            // Create temporary render textures using the target descriptor from above.
            for (int i = 0; i < _blurSettings.blurPasses; i++)
            {
                cmd.GetTemporaryRT(_mipUpID[i], _targetsDescriptor, FilterMode.Bilinear);
                cmd.GetTemporaryRT(_mipDownID[i], _targetsDescriptor, FilterMode.Bilinear);
                _targetsDescriptor.width = Mathf.Max(1, _targetsDescriptor.width >> 1);
                _targetsDescriptor.height = Mathf.Max(1, _targetsDescriptor.height >> 1);
            }

            _material.SetFloat(BlurSizeProperty, _blurSettings.blurSize);

            if (_featureSettings.dithering)
            {
                _material.EnableKeyword("ENABLE_DITHERING");

                _material.SetVector(DitheringParamsProperty, new Vector4(
                    renderingData.cameraData.camera.scaledPixelWidth / (float)_blueNoiseTexture.width,
                    renderingData.cameraData.camera.scaledPixelHeight / (float)_blueNoiseTexture.height,
                    1, // Unused
                    1  // Unused
                ));
            }
            else
            {
                _material.DisableKeyword("ENABLE_DITHERING");
            }

            // Grab the color buffer from the renderer camera color target.
            _colorTarget = renderingData.cameraData.renderer.cameraColorTarget;
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
                // Downsample pass
                RenderTargetIdentifier lastDown = _colorTarget;
                for (int i = 0; i < _blurSettings.blurPasses; i++)
                {
                    if (i == 0)
                    {
                        Blit(cmd, lastDown, _mipDownTarget[i], _material, (int)ShaderPass.DownSampleAntiFlicker);
                    }
                    else
                    {
                        Blit(cmd, lastDown, _mipDownTarget[i], _material, (int)ShaderPass.DownSample);
                    }

                    lastDown = _mipDownTarget[i];
                }

                // Upsample pass
                RenderTargetIdentifier lastUp = _mipDownTarget[_blurSettings.blurPasses - 1];
                for (int i = _blurSettings.blurPasses - 2; i > 0; i--)
                {
                    Blit(cmd, lastUp, _mipUpTarget[i], _material, (int)ShaderPass.UpSample);

                    lastUp = _mipUpTarget[i];
                }

                // Final pass with some additional filtering (blending with camera texture, dithering...)
                Blit(cmd, lastUp, _mipUpTarget[0], _material, (int)ShaderPass.FinalUpSample);

                if (_featureSettings.copyToCameraFramebuffer)
                {
                    Blit(cmd, _mipUpTarget[0], _colorTarget, _material);
                }
                else
                {
                    cmd.SetGlobalTexture(_featureSettings.blurTextureName, _mipUpTarget[0]);
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
            for (int i = 0; i < _blurSettings.blurPasses; i++)
            {
                cmd.ReleaseTemporaryRT(_mipUpID[i]);
                cmd.ReleaseTemporaryRT(_mipDownID[i]);
            }
        }

        public void Dispose()
        {
            CoreUtils.Destroy(_material);
        }
    }
}