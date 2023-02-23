using DeepDreams.ScriptableObjects;
using UnityEngine;

namespace DeepDreams.UI
{
    public class ViewAudioModule : MonoBehaviour
    {
        [SerializeField] private AudioEventSO uiAudioPause;

        private void OnEnable()
        {
            ViewInputModule.OnCancelEvent += OnCancel;
        }

        private void OnDisable()
        {
            ViewInputModule.OnCancelEvent -= OnCancel;
        }

        private void OnCancel(bool isPaused)
        {
            uiAudioPause?.Play();
        }
    }
}