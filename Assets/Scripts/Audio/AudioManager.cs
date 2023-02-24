using System.Collections.Generic;
using DeepDreams.ScriptableObjects.Audio;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace DeepDreams.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private List<BusReference> busReferences;
        private Dictionary<BusReference, Bus> _audioBuses;
        public static AudioManager instance { get; private set; }

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("Found more than one Audio Manager in the scene.");
            }

            instance = this;

            _audioBuses = new Dictionary<BusReference, Bus>();
            foreach (BusReference bus in busReferences) _audioBuses[bus] = RuntimeManager.GetBus(bus.busPath);
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