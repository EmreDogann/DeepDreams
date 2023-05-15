using DeepDreams.ScriptableObjects.Audio;
using DeepDreams.Services;
using UnityEngine;

namespace DeepDreams.Audio
{
    public interface IAudioManager : IService
    {
        public void PlayOneShot(AudioReference sound, Vector3 worldPos = default);
        public void PlayOneShotAttached(AudioReference sound, GameObject targetGameObject);
        public void SetVolume(BusReference bus, float volume);
        public float GetVolume(BusReference bus);
        public void StopAllEvents(BusReference bus, bool immediate);
        public void Pause(BusReference bus, bool paused);
        public void Resume(BusReference bus);
    }
}