using UnityEngine;

namespace DeepDreams.Water
{
    public class WaterSimulation : MonoBehaviour
    {
        [SerializeField] private CustomRenderTexture texture;
        [SerializeField] private int iterationPerFrame = 5;

        // Start is called before the first frame update
        private void Start()
        {
            texture.initializationColor = Color.black;
            texture.Initialize();
        }

        // Update is called once per frame
        private void Update()
        {
            texture.Update(iterationPerFrame);
        }
    }
}