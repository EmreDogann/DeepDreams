using DeepDreams.Audio;
using DeepDreams.DataPersistence;
using DeepDreams.DataPersistence.Data;
using DeepDreams.ScriptableObjects.Audio;
using DeepDreams.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeepDreams.UI.Components
{
    public class AudioSlider : MonoBehaviour, IDataPersistence<SettingsData>
    {
        [SerializeField] private TMP_Text sliderTextComponent;
        [SerializeField] private BusReference bus;
        [SerializeField] private Slider slider;

        [SerializeField] private bool persistData;

        [SerializeField] private DataPersistenceFieldReference<SettingsData> selectedDataField;
        private FieldAccessor _dataFieldAccessor;

        public void SetSliderValue(float sliderValue)
        {
            sliderTextComponent.text = sliderValue.ToString();
            AudioManager.instance.SetVolume(bus, sliderValue * 0.01f);
        }

        private void SetVolume(float volume)
        {
            slider.value = volume;
            SetSliderValue(volume);
        }

        public void InitDataPersistence()
        {
            if (!persistData) return;
            _dataFieldAccessor = DataPersistenceManager.instance.GetFieldAccessor<SettingsData>(selectedDataField.selectedName);
        }

        public void LoadData(PersistentData persistentData)
        {
            if (!persistData) return;

            SettingsData settingsData = (SettingsData)persistentData;
            SetVolume((float)_dataFieldAccessor.Get(settingsData) * 100.0f);

            // Debug.Log($"Loaded {selectedDataField.selectedName}: {(float)dataFieldAccessor.Get(settingsData) * 100.0f}");
        }

        public void SaveData(PersistentData persistentData)
        {
            if (!persistData) return;

            SettingsData settingsData = (SettingsData)persistentData;
            _dataFieldAccessor.Set(settingsData, AudioManager.instance.GetVolume(bus));

            // Debug.Log($"Saving {selectedDataField.selectedName}: {dataFieldAccessor.Get(settingsData)}");
        }
    }
}