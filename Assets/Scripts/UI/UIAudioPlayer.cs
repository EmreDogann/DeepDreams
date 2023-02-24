using DeepDreams.Audio;
using DeepDreams.ScriptableObjects.Audio;
using DeepDreams.ScriptableObjects.Events;
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
        [SerializeField] private AudioReference startGameAudio;

        private BoolEventListener _onGamePausedEvent;

        private void Awake()
        {
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
                AudioManager.instance.PlayOneShot(backAudio);
            }
        }

        private void OnGamePaused(bool isPaused)
        {
            if (isPaused)
            {
                AudioManager.instance.StopAllEvents(masterBus, false);
                AudioManager.instance.PlayOneShot(pauseAudio);
            }
        }

        private void OnUIHover(AudioReference audioReference)
        {
            AudioReference audio = audioReference == null ? hoverAudio : audioReference;
            AudioManager.instance.PlayOneShot(audio);
        }

        private void OnUIClick(AudioReference audioReference)
        {
            AudioReference audio = audioReference == null ? clickAudio : audioReference;
            AudioManager.instance.PlayOneShot(audio);
        }

        private void OnUIBack(AudioReference audioReference)
        {
            AudioReference audio = audioReference == null ? backAudio : audioReference;
            AudioManager.instance.PlayOneShot(audio);
        }

        private void OnUIPause(AudioReference audioReference)
        {
            AudioReference audio = audioReference == null ? pauseAudio : audioReference;
            AudioManager.instance.PlayOneShot(audio);
        }

        private void OnUIStartGame(AudioReference audioReference)
        {
            AudioReference audio = audioReference == null ? startGameAudio : audioReference;
            AudioManager.instance.PlayOneShot(audio);
        }
    }
}