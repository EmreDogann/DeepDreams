using System.Collections.Generic;
using DeepDreams.ScriptableObjects.Audio;
using DeepDreams.Services;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace DeepDreams.Audio
{
    public class AudioManager : MonoBehaviour, IAudioManager
    {
        [SerializeField] private List<BusReference> busReferences;
        private Dictionary<BusReference, Bus> _audioBuses;

        private void Awake()
        {
            ServiceLocator.Instance.Register<IAudioManager>(this);

            _audioBuses = new Dictionary<BusReference, Bus>();

            foreach (BusReference bus in busReferences)
            {
                _audioBuses[bus] = RuntimeManager.GetBus(bus.busPath);
            }
        }

        public void PlayOneShot(AudioReference sound, Vector3 worldPos = default)
        {
            RuntimeManager.PlayOneShot(sound.audioReference, worldPos);
        }

        public void PlayOneShotAttached(AudioReference sound, GameObject targetGameObject)
        {
            RuntimeManager.PlayOneShotAttached(sound.audioReference, targetGameObject);
        }

        public void SetVolume(BusReference bus, float volume)
        {
            _audioBuses[bus].setVolume(volume);
        }

        public float GetVolume(BusReference bus)
        {
            _audioBuses[bus].getVolume(out float volume);
            return volume;
        }

        public void StopAllEvents(BusReference bus, bool immediate)
        {
            if (immediate)
            {
                _audioBuses[bus].stopAllEvents(STOP_MODE.IMMEDIATE);
            }
            else
            {
                _audioBuses[bus].stopAllEvents(STOP_MODE.ALLOWFADEOUT);
            }
        }

        public void Pause(BusReference bus, bool paused)
        {
            _audioBuses[bus].setPaused(paused);
        }

        public void Resume(BusReference bus)
        {
            Pause(bus, false);
        }
    }
}