using DeepDreams.ScriptableObjects.Events;
using FMODUnity;
using UnityEngine;

namespace DeepDreams.Audio
{
    public class AudioManager : MonoBehaviour
    {
        private BoolEventListener _onGamePausedEvent;
        public static AudioManager instance { get; private set; }

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("Found more than one Audio Manager in the scene.");
            }

            instance = this;

            _onGamePausedEvent = GetComponent<BoolEventListener>();
        }

        private void OnEnable()
        {
            _onGamePausedEvent.Response.AddListener(PauseAllSound);
        }

        private void OnDisable()
        {
            _onGamePausedEvent.Response.RemoveListener(PauseAllSound);
        }

        public void PlayOneShot(EventReference sound, Vector3 worldPos = default)
        {
            RuntimeManager.PlayOneShot(sound, worldPos);
        }

        public void PlayOneShotAttached(EventReference sound, GameObject targetGameObject)
        {
            RuntimeManager.PlayOneShotAttached(sound, targetGameObject);
        }

        public void PauseAllSound(bool paused)
        {
            // RuntimeManager.PauseAllEvents(!paused);
        }
    }
}