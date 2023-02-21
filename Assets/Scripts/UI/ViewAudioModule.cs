using DeepDreams.Audio;
using UnityEngine;

namespace DeepDreams.UI
{
    public class ViewAudioModule : MonoBehaviour
    {
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
            AudioManager.instance.PlayOneShot(FMODEvents.instance.UI_Pause, Vector3.zero);
        }
    }
}