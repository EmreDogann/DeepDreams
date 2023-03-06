using MyBox;
using UnityEngine;

namespace DeepDreams.Water
{
    public class WaterSimulation : MonoBehaviour
    {
        [Separator("General")]
        [SerializeField] private float waterSize;
        [SerializeField] private Camera waterCollisionCamera;

        [Separator("Textures")]
        [SerializeField] private CustomRenderTexture simulationTexture;
        [SerializeField] private int simulationTextureSize;
        [SerializeField] private RenderTexture collisionTexture;
        [SerializeField] private int collisionTextureSize;

        [Separator("Simulation Update")]
        [SerializeField] private int iterationsPerFrame = 1;

        [Separator("Simulation Parameters")]
        [SerializeField] [Range(0.0f, 0.1f)] private float waveSpeed;
        [ReadOnly] [SerializeField] private float a;
        [SerializeField] [Range(0.5f, 1.0f)] private float waveAttenuation;
        [SerializeField] private float simulationAmplitude;
        [SerializeField] private float simulationUVScale;

        [SerializeField] private bool enableDebugView;

        private Material waterMaterial;

        private int _aID;
        private int _attenuationID;
        private int _amplitudeID;
        private int _uvScaleID;
        private int _simulationMapWidthID;
        private int _simulationMapHeightID;

        // Start is called before the first frame update
        private void Start()
        {
            waterMaterial = GetComponent<MeshRenderer>().material;

            _aID = Shader.PropertyToID("_a");
            _attenuationID = Shader.PropertyToID("_Attenuation");
            _amplitudeID = Shader.PropertyToID("_Amplitude");
            _uvScaleID = Shader.PropertyToID("_UVScale");
            _simulationMapWidthID = Shader.PropertyToID("_HeightMap_Width");
            _simulationMapHeightID = Shader.PropertyToID("_HeightMap_Height");

            transform.localScale = new Vector3(waterSize, 1, waterSize);
            waterCollisionCamera.orthographicSize = waterSize;
            waterCollisionCamera.targetTexture = collisionTexture;

            simulationTexture.Release();
            simulationTexture.initializationColor = Color.black;
            simulationTexture.width = simulationTextureSize;
            simulationTexture.height = simulationTextureSize;
            simulationTexture.Initialize();

            CalculateA();
            simulationTexture.material.SetFloat(_aID, a);
            simulationTexture.material.SetFloat(_amplitudeID, simulationAmplitude);
            simulationTexture.material.SetFloat(_uvScaleID, simulationUVScale);
            simulationTexture.material.SetFloat(_attenuationID, waveAttenuation);

            waterMaterial.SetFloat(_simulationMapWidthID, simulationTexture.width);
            waterMaterial.SetFloat(_simulationMapHeightID, simulationTexture.height);
        }

        private void OnValidate()
        {
            if (simulationTexture.width != simulationTextureSize)
            {
                simulationTexture.Release();
                simulationTexture.width = simulationTextureSize;
                simulationTexture.height = simulationTextureSize;
                simulationTexture.Initialize();

                if (waterMaterial != null)
                {
                    waterMaterial.SetFloat(_simulationMapWidthID, simulationTexture.width);
                    waterMaterial.SetFloat(_simulationMapHeightID, simulationTexture.height);
                }
            }

            if (collisionTexture.width != collisionTextureSize)
            {
                collisionTexture.Release();
                collisionTexture.width = collisionTextureSize;
                collisionTexture.height = collisionTextureSize;
                collisionTexture.Create();
            }

            transform.localScale = new Vector3(waterSize, 1, waterSize);
            waterCollisionCamera.orthographicSize = waterSize;
            waterCollisionCamera.targetTexture = collisionTexture;

            CalculateA();
            simulationTexture.material.SetFloat(_aID, a);
            simulationTexture.material.SetFloat(_amplitudeID, simulationAmplitude);
            simulationTexture.material.SetFloat(_uvScaleID, simulationUVScale);
            simulationTexture.material.SetFloat(_attenuationID, waveAttenuation);
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

        private void FixedUpdate()
        {
            simulationTexture.Update(iterationsPerFrame);
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