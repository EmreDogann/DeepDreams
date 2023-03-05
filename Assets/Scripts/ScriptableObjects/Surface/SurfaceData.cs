using DeepDreams.ScriptableObjects.Audio;
using UnityEngine;

namespace DeepDreams.ScriptableObjects.Surface
{
    [CreateAssetMenu(fileName = "New Surface Data", menuName = "Surface Data", order = 0)]
    public class SurfaceData : ScriptableObject
    {
        public AudioReference walkSound;
        public AudioReference runSound;
    }
}