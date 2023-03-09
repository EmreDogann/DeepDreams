using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Rendering;

namespace DeepDreams.Water
{
    public class WaterSimulation : MonoBehaviour
    {
        [Separator("General")]
        [SerializeField] private float waterSize;

        [Separator("Textures")]
        [SerializeField] private CustomRenderTexture simulationTexture;
        [SerializeField] private int simulationTextureSize;
        [SerializeField] private RenderTexture collisionTexture;
        [SerializeField] private int collisionTextureSize;
        [SerializeField] private LayerMask collisionLayer;

        [Separator("Simulation Update")]
        [SerializeField] private int iterationsPerFrame = 1;

        [Separator("Simulation Parameters")]
        [SerializeField] [Range(0.0f, 0.1f)] private float waveSpeed;
        [ReadOnly] [SerializeField] private float a;
        [SerializeField] [Range(0.5f, 1.0f)] private float waveAttenuation;
        [SerializeField] private float simulationAmplitude;
        [SerializeField] private float simulationUVScale;

        [SerializeField] private bool enableDebugView;

        private Material _waterMaterial;
        private CommandBuffer _cmdBuffer;
        private MeshRenderer[] _collisionMeshes;

        private int _aID;
        private int _attenuationID;
        private int _amplitudeID;
        private int _uvScaleID;
        private int _simulationMapWidthID;
        private int _simulationMapHeightID;

        private int _frameCountAtSceneStart;

        // Start is called before the first frame update
        private void Start()
        {
            _waterMaterial = GetComponent<MeshRenderer>().sharedMaterial;

            _aID = Shader.PropertyToID("_A");
            _attenuationID = Shader.PropertyToID("_Attenuation");
            _amplitudeID = Shader.PropertyToID("_Amplitude");
            _uvScaleID = Shader.PropertyToID("_UVScale");
            _simulationMapWidthID = Shader.PropertyToID("_HeightMap_Width");
            _simulationMapHeightID = Shader.PropertyToID("_HeightMap_Height");

            transform.localScale = new Vector3(waterSize, waterSize, waterSize);

            simulationTexture.Release();
            simulationTexture.initializationColor = Color.black;
            simulationTexture.width = simulationTextureSize;
            simulationTexture.height = simulationTextureSize;
            simulationTexture.Initialize();

            collisionTexture.Release();
            collisionTexture.width = collisionTextureSize;
            collisionTexture.height = collisionTextureSize;
            collisionTexture.Create();

            CalculateA();
            simulationTexture.material.SetFloat(_aID, a);
            simulationTexture.material.SetFloat(_amplitudeID, simulationAmplitude);
            simulationTexture.material.SetFloat(_uvScaleID, simulationUVScale);
            simulationTexture.material.SetFloat(_attenuationID, waveAttenuation);

            _waterMaterial.SetFloat(_simulationMapWidthID, simulationTexture.width);
            _waterMaterial.SetFloat(_simulationMapHeightID, simulationTexture.height);

            _collisionMeshes = FindMeshesWithLayer(collisionLayer);

            _cmdBuffer = new CommandBuffer();
            _cmdBuffer.name = "Water Collision";
            _cmdBuffer.SetRenderTarget(collisionTexture);

            _cmdBuffer.SetViewMatrix(Matrix4x4.TRS(
                transform.position,
                Quaternion.Euler(-90.0f, 0.0f, 0.0f),
                Vector3.one)
            );

            _cmdBuffer.SetProjectionMatrix(Matrix4x4.Ortho(
                -waterSize, waterSize,
                -waterSize, waterSize,
                0.0f, 10.0f)
            );

            _cmdBuffer.ClearRenderTarget(true, true, Color.black, 1.0f);

            for (int i = 0; i < _collisionMeshes.Length; i++)
            {
                _cmdBuffer.DrawRenderer(_collisionMeshes[i], _collisionMeshes[i].sharedMaterial, _collisionMeshes[i].subMeshStartIndex,
                    0);
            }

            _frameCountAtSceneStart = Time.frameCount;
        }

        private void OnDestroy()
        {
            _cmdBuffer.Release();
            simulationTexture.Release();
            collisionTexture.Release();
        }

        private void OnValidate()
        {
            if (simulationTexture.width != simulationTextureSize)
            {
                simulationTexture.Release();
                simulationTexture.width = simulationTextureSize;
                simulationTexture.height = simulationTextureSize;
                simulationTexture.Initialize();

                if (_waterMaterial != null)
                {
                    _waterMaterial.SetFloat(_simulationMapWidthID, simulationTexture.width);
                    _waterMaterial.SetFloat(_simulationMapHeightID, simulationTexture.height);
                }
            }

            if (collisionTexture.width != collisionTextureSize)
            {
                collisionTexture.Release();
                collisionTexture.width = collisionTextureSize;
                collisionTexture.height = collisionTextureSize;
                collisionTexture.Create();
            }

            transform.localScale = new Vector3(waterSize, waterSize, waterSize);

            CalculateA();
            simulationTexture.material.SetFloat(_aID, a);
            simulationTexture.material.SetFloat(_amplitudeID, simulationAmplitude);
            simulationTexture.material.SetFloat(_uvScaleID, simulationUVScale);
            simulationTexture.material.SetFloat(_attenuationID, waveAttenuation);

            if (_cmdBuffer != null) _cmdBuffer.Release();
            _cmdBuffer = new CommandBuffer();
            _cmdBuffer.name = "Water Collision";
            _cmdBuffer.SetRenderTarget(collisionTexture);

            _cmdBuffer.SetViewMatrix(Matrix4x4.TRS(
                transform.position,
                Quaternion.Euler(-90.0f, 0.0f, 0.0f),
                Vector3.one)
            );

            _cmdBuffer.SetProjectionMatrix(Matrix4x4.Ortho(
                -waterSize, waterSize,
                -waterSize, waterSize,
                0.0f, 10.0f)
            );

            _cmdBuffer.ClearRenderTarget(true, true, Color.black, 1.0f);

            if (_collisionMeshes != null)
            {
                for (int i = 0; i < _collisionMeshes.Length; i++)
                {
                    _cmdBuffer.DrawRenderer(_collisionMeshes[i], _collisionMeshes[i].sharedMaterial, _collisionMeshes[i].subMeshStartIndex,
                        0);
                }
            }
        }

        private void Update()
        {
            // Skip first frame of scene load to allow render textures and materials to fully set their initial values before rendering. 
            if (Time.frameCount - _frameCountAtSceneStart > 1) DrawCollisionMeshes();
        }

        private void FixedUpdate()
        {
            simulationTexture.Update(iterationsPerFrame);
        }

        private void DrawCollisionMeshes()
        {
            Graphics.ExecuteCommandBuffer(_cmdBuffer);
        }

        private void CalculateA()
        {
            // h is known as the texel size (this assumes that the texture is square with size X = size Y).
            // h = 1 / textureSize
            // a = c^2 * deltaT^2 / h^2
            //   = c^2 * deltaT^2 / (1 / textureSize)^2
            //   = c^2 * deltaT^2 * textureSize^2
            a = waveSpeed * Time.fixedDeltaTime * simulationTexture.width;
            a *= a; // This will effectively square every component.

            if (a > 0.5)
            {
                Debug.LogWarning(
                    $"<color=cyan>WaterSimulation.cs</color>: a is {a}. It cannot be above 0.5 in order to keep a stable simulation. Clamping to 0.5...");
                a = 0.5f;
            }
        }

        private MeshRenderer[] FindMeshesWithLayer(int layerMask)
        {
            var goArray = FindObjectsOfType(typeof(GameObject), true) as GameObject[];
            var goList = new List<MeshRenderer>();

            // Only works with a single layer in the layer mask.
            int layer = (int)Mathf.Log(layerMask, 2);

            if (goArray != null)
            {
                for (int i = 0; i < goArray.Length; i++)
                {
                    if (goArray[i].layer == layer)
                    {
                        if (goArray[i].TryGetComponent(out MeshRenderer meshRenderer))
                            goList.Add(meshRenderer);
                    }
                }
            }

            if (goList.Count == 0) return null;
            return goList.ToArray();
        }

        private void OnGUI()
        {
            if (enableDebugView)
            {
                GUI.DrawTexture(new Rect(0, 0, 512, 512), collisionTexture, ScaleMode.ScaleToFit, false, 1);
                GUI.DrawTexture(new Rect(0, 512, 512, 512), simulationTexture.GetDoubleBufferRenderTexture(), ScaleMode.ScaleToFit, false,
                    1);
            }
        }
    }
}