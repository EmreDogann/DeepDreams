using DeepDreams.Audio;
using FMODUnity;
using UnityEngine;

namespace DeepDreams.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Audio Event", fileName = "New Audio Event")]
    public class AudioEventSO : ScriptableObject
    {
        [field: SerializeField] public EventReference audioReference;

        public void Play()
        {
            AudioManager.instance.PlayOneShot(audioReference);
        }
    }
}