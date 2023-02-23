using TMPro;
using UnityEngine;

namespace DeepDreams.UI
{
    public class Slider : MonoBehaviour
    {
        [SerializeField] private TMP_Text sliderTextComponent;

        public void SetSliderValue(float sliderValue)
        {
            sliderTextComponent.text = Mathf.Round(sliderValue * 100).ToString();
        }
    }
}