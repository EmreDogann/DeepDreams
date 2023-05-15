using DeepDreams.SaveLoad;
using DeepDreams.SaveLoad.Data;
using DeepDreams.Services;
using DeepDreams.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeepDreams.UI.Components
{
    public class SliderController : MonoBehaviour, ISaveable<SettingsData>
    {
        [SerializeField] protected TMP_Text sliderTextComponent;
        [SerializeField] protected Slider slider;

        [SerializeField] protected bool persistData;

        [SerializeField] protected SaveDataFieldReference<SettingsData> selectedDataField;
        protected FieldAccessor _dataFieldAccessor;

        protected ISaveManager _saveManager;

        private void Start()
        {
            _saveManager = ServiceLocator.Instance.GetService<ISaveManager>();

            if (!persistData)
            {
                return;
            }

            // _saveManager.RegisterListener(this);

            _dataFieldAccessor =
                _saveManager.GetFieldAccessor<SettingsData>(selectedDataField.selectedName);
        }

        private void OnDestroy()
        {
            // _saveManager.UnregisterListener(this);
        }

        public virtual void SetSliderValue(float sliderValue)
        {
            sliderTextComponent.text = sliderValue.ToString();
            slider.value = sliderValue;
        }

        public virtual void LoadData(SettingsData saveData) {}

        public virtual void SaveData(SettingsData saveData) {}
    }
}