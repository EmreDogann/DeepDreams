using FMODUnity;
using UnityEngine;

namespace DeepDreams.ScriptableObjects.Audio
{
    [CreateAssetMenu(menuName = "Audio/Audio Event", fileName = "New Audio Event")]
    public class AudioReference : ScriptableObject
    {
        [field: SerializeField] public EventReference audioReference;
    }
}