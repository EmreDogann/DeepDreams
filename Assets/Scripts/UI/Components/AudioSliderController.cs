using DeepDreams.Audio;
using DeepDreams.SaveLoad.Data;
using DeepDreams.ScriptableObjects.Audio;
using DeepDreams.Services;
using UnityEngine;

namespace DeepDreams.UI.Components
{
    public class AudioSliderController : SliderController
    {
        [SerializeField] private BusReference bus;

        public override void SetSliderValue(float sliderValue)
        {
            base.SetSliderValue(sliderValue);
            ServiceLocator.Instance.GetService<IAudioManager>().SetVolume(bus, sliderValue * 0.01f);
        }

        public override void LoadData(SettingsData saveData)
        {
            if (!persistData)
            {
                return;
            }

            SetSliderValue((float)_dataFieldAccessor.Get(saveData) * 100.0f);

            // Debug.Log($"Loaded {selectedDataField.selectedName}: {(float)dataFieldAccessor.Get(settingsData) * 100.0f}");
        }

        public override void SaveData(SettingsData saveData)
        {
            if (!persistData)
            {
                return;
            }

            _dataFieldAccessor.Set(saveData, slider.value * 0.01f);

            // Debug.Log($"Saving {selectedDataField.selectedName}: {dataFieldAccessor.Get(settingsData)}");
        }
    }
}