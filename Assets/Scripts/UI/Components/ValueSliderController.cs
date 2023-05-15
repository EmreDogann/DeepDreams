using DeepDreams.SaveLoad.Data;

namespace DeepDreams.UI.Components
{
    public class ValueSliderController : SliderController
    {
        public override void LoadData(SettingsData saveData)
        {
            if (!persistData)
            {
                return;
            }

            slider.onValueChanged.Invoke((float)_dataFieldAccessor.Get(saveData));

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