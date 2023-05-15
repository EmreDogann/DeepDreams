using DeepDreams.SaveLoad.Data;
using UnityEngine;

namespace DeepDreams.UI.Components
{
    public class FOVSliderController : SliderController
    {
        [SerializeField] private new Camera camera;

        public override void SetSliderValue(float sliderValue)
        {
            base.SetSliderValue(sliderValue);
            camera.fieldOfView = Camera.HorizontalToVerticalFieldOfView(sliderValue, camera.aspect);
        }

        public override void LoadData(SettingsData saveData)
        {
            if (!persistData)
            {
                return;
            }

            slider.onValueChanged.Invoke((float)_dataFieldAccessor.Get(saveData));
            // _saveManager.Save<SettingsData>(nameof(SettingsData.hFieldOfView));

            // Debug.Log($"Loaded {selectedDataField.selectedName}: {(float)dataFieldAccessor.Get(settingsData) * 100.0f}");
        }

        public override void SaveData(SettingsData saveData)
        {
            if (!persistData)
            {
                return;
            }

            _dataFieldAccessor.Set(saveData, slider.value);

            // Debug.Log($"Saving {selectedDataField.selectedName}: {dataFieldAccessor.Get(settingsData)}");
        }
    }
}