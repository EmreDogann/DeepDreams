using DeepDreams.Audio;
using DeepDreams.ScriptableObjects.Audio;
using DeepDreams.ScriptableObjects.Events.UnityEvents;
using DeepDreams.Services;
using DeepDreams.UI.Components.Buttons;
using MyBox;
using UnityEngine;

namespace DeepDreams.UI
{
    public class UIAudioPlayer : MonoBehaviour
    {
        [Separator("General")]
        [SerializeField] private BusReference masterBus;

        [Separator("UI Audio Effects")]
        [SerializeField] private AudioReference clickAudio;
        [SerializeField] private AudioReference hoverAudio;
        [SerializeField] private AudioReference backAudio;
        [SerializeField] private AudioReference pauseAudio;

        private BoolEventListener _onGamePausedEvent;
        private IAudioManager _audioManager;

        private void Awake()
        {
            _audioManager = ServiceLocator.Instance.GetService<IAudioManager>();
            _onGamePausedEvent = GetComponent<BoolEventListener>();
        }

        private void OnEnable()
        {
            MenuButton.OnButtonHover += OnUIHover;
            MenuButton.OnButtonClick += OnUIClick;
            UIInputModule.OnCancelEvent += OnCancel;
            _onGamePausedEvent.Response.AddListener(OnGamePaused);
        }

        private void OnDisable()
        {
            MenuButton.OnButtonHover -= OnUIHover;
            MenuButton.OnButtonClick -= OnUIClick;
            UIInputModule.OnCancelEvent -= OnCancel;
            _onGamePausedEvent.Response.RemoveListener(OnGamePaused);
        }

        private void OnCancel(bool isPaused)
        {
            if (isPaused)
            {
                _audioManager.PlayOneShot(backAudio);
            }
        }

        private void OnGamePaused(bool isPaused)
        {
            if (isPaused)
            {
                _audioManager.StopAllEvents(masterBus, false);
                _audioManager.PlayOneShot(pauseAudio);
            }
        }

        private void OnUIHover()
        {
            AudioReference audio = hoverAudio;
            _audioManager.PlayOneShot(audio);
        }

        private void OnUIClick()
        {
            AudioReference audio = clickAudio;
            _audioManager.PlayOneShot(audio);
        }
    }
}