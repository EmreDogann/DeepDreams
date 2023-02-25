using System;
using DeepDreams.UI.Effects;
using UnityEngine.EventSystems;

namespace DeepDreams.UI.Components.Buttons
{
    public class OptionsCategoryButton : MenuButton
    {
        public event Action<OptionsCategoryButton> OnCategoryClicked;
        public event Action<OptionsCategoryButton> OnCategoryChanged;

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            OnCategoryClicked?.Invoke(this);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            OnCategoryChanged?.Invoke(this);
        }

        public void Toggle(bool isSelected)
        {
            _isSelected = isSelected;
            _isHighlighted = isSelected ? false : _isHighlighted;
            ButtonClicked();
            foreach (IButtonEffect effect in _buttonEffects) effect.OnToggle(isSelected);
        }
    }
}