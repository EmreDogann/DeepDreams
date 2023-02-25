using DeepDreams.Audio;
using DeepDreams.ScriptableObjects.Audio;
using TMPro;
using UnityEngine;

namespace DeepDreams.UI.Components
{
    public class AudioSlider : MonoBehaviour
    {
        [SerializeField] private TMP_Text sliderTextComponent;
        [SerializeField] private BusReference bus;

        public void SetSliderValue(float sliderValue)
        {
            sliderTextComponent.text = Mathf.Round(sliderValue).ToString();
            AudioManager.instance.SetVolume(bus, sliderValue * 0.01f);
        }
    }
}