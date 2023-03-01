using UnityEngine;

namespace DeepDreams.Water
{
    public class WaterManager : MonoBehaviour
    {
        public Material waveMaterial;
        public Texture2D waveTexture;

        // Wave state information
        private float[][] _waveN, _waveNm1, _waveNp1;

        // Width and height of texture.
        private const float Lx = 10;
        private const float Ly = 10;

        // Density
        [SerializeField] private float dx = 0.1f;
        private float dy => dx;

        // Resolution
        private int _nx, _ny;

        public float CFL = 0.5f;
        public float c = 1.0f;

        private float _dt; // Time step
        private float _t; // Current time 

        private readonly int _mainTex = Shader.PropertyToID("_MainTex");
        private readonly int _displacement = Shader.PropertyToID("_Displacement");

        // Start is called before the first frame update
        private void Start()
        {
            _nx = Mathf.FloorToInt(Lx / dx);
            _ny = Mathf.FloorToInt(Ly / dy);
            waveTexture = new Texture2D(_nx, _ny, TextureFormat.RGBA32, false);

            // Initialize 2D array (matrix).
            _waveN = new float[_nx][];
            _waveNm1 = new float[_nx][];
            _waveNp1 = new float[_nx][];

            for (int i = 0; i < _nx; i++)
            {
                _waveN[i] = new float[_ny];
                _waveNm1[i] = new float[_ny];
                _waveNp1[i] = new float[_ny];
            }

            waveMaterial.SetTexture(_mainTex, waveTexture); // Color texture.
            waveMaterial.SetTexture(_displacement, waveTexture); // Displacement texture.
        }

        private void WaveStep()
        {
            _dt = CFL * dx / c;
            _t += dx;

            for (int i = 0; i < _nx; i++)
            {
                for (int j = 0; j < _ny; j++)
                {
                    _waveNm1[i][j] = _waveN[i][j]; // Copy state N to state N-1.
                    _waveN[i][j] = _waveNp1[i][j]; // Copy state N+1 to state N.
                }
            }

            for (int i = 1; i < _nx - 1; i++) // Do not process edges.
            {
                for (int j = 1; j < _ny - 1; j++)
                {
                    _waveNm1[i][j] = _waveN[i][j]; // Copy state N to state N-1.
                    _waveN[i][j] = _waveNp1[i][j]; // Copy state N+1 to state N.
                }
            }
        }

        // Update is called once per frame
        private void Update() {}
    }
}