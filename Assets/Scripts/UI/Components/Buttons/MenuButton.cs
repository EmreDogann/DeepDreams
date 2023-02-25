using System;
using System.Collections;
using DeepDreams.Audio;
using DeepDreams.ScriptableObjects.Audio;
using DeepDreams.UI.Effects;
using MyBox;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeepDreams.UI.Components.Buttons
{
    public class MenuButton : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler,
        IPointerDownHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        public enum ButtonStatus
        {
            Normal,
            Disabled,
            Highlighted,
            Pressed,
            Selected
        }

        [Separator("General")]
        [OverrideLabel("Interactable")] public bool isEnabled = true;
        [OverrideLabel("Clickable")] public bool isClickable = true;
        [OverrideLabel("Toggleable")] public bool isToggleable;

        public Graphic targetGraphic;
        public ColorBlock colorBlock = new ColorBlock();

        [Range(0.0f, 5.0f)] public float transitionTime = 0.1f;
        [Space]
        public UIClickEvent onClickEvent;

        [Separator("Button Effects")]
        protected IButtonEffect[] _buttonEffects;

        [Separator("Audio Overrides")]
        [SerializeField] private bool playHoverAudio = true;
        [ReadOnly(nameof(isClickable), false, false)] [SerializeField] private bool playClickAudio = true;

        [OverrideLabel("Hover Audio")] [SerializeField] private AudioReference uiAudioHoverOverride;
        [ReadOnly(nameof(isClickable), false, false)] [OverrideLabel("Click Audio")]
        [SerializeField] private AudioReference uiAudioClickOverride;

        private ButtonStatus _buttonStatus = ButtonStatus.Normal;
        protected bool _isHighlighted;
        protected bool _isSelected;

        public static event Action OnButtonHover;
        public static event Action OnButtonClick;

        private void Awake()
        {
            if (targetGraphic == null) targetGraphic = GetComponentInChildren<TextMeshProUGUI>();

            if (!isEnabled)
            {
                targetGraphic.color = colorBlock.disabledColor;
                _buttonStatus = ButtonStatus.Disabled;
            }
            else targetGraphic.color = colorBlock.normalColor;

            _buttonEffects = GetComponents<IButtonEffect>();
        }

        public void OnBeginDrag(PointerEventData eventData) {}

        public void OnDrag(PointerEventData eventData) {}

        public void OnEndDrag(PointerEventData eventData) {}

        protected void ButtonClicked()
        {
            if (_isSelected)
            {
                _buttonStatus = ButtonStatus.Selected;
                StartCoroutine(TransitionColor(colorBlock.selectedColor, transitionTime));
            }
            else
            {
                if (_isHighlighted)
                {
                    _buttonStatus = ButtonStatus.Highlighted;
                    StartCoroutine(TransitionColor(colorBlock.highlightedColor, transitionTime));
                }
                else
                {
                    _buttonStatus = ButtonStatus.Normal;
                    StartCoroutine(TransitionColor(colorBlock.normalColor, transitionTime));
                }
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!isEnabled || !isClickable) return;

            if (isToggleable) _isSelected = !_isSelected;
            ButtonClicked();

            onClickEvent?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isEnabled || !isClickable) return;

            _buttonStatus = ButtonStatus.Pressed;
            StartCoroutine(TransitionColor(colorBlock.pressedColor, transitionTime));

            if (playClickAudio)
            {
                if (uiAudioClickOverride) AudioManager.instance.PlayOneShot(uiAudioClickOverride);
                else OnButtonClick?.Invoke();
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!isEnabled || IsSelected()) return;

            _isHighlighted = true;
            _buttonStatus = ButtonStatus.Highlighted;

            StartCoroutine(TransitionColor(colorBlock.highlightedColor, transitionTime));

            if (playHoverAudio)
            {
                if (uiAudioHoverOverride) AudioManager.instance.PlayOneShot(uiAudioHoverOverride);
                else OnButtonHover?.Invoke();
            }

            foreach (IButtonEffect effect in _buttonEffects) effect.OnHoverEnter();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (!isEnabled || IsSelected()) return;

            _isHighlighted = false;
            _buttonStatus = ButtonStatus.Normal;

            StartCoroutine(TransitionColor(colorBlock.normalColor, transitionTime));

            foreach (IButtonEffect effect in _buttonEffects) effect.OnHoverExit();
        }

        public bool IsSelected()
        {
            return _buttonStatus == ButtonStatus.Selected;
        }

        private IEnumerator TransitionColor(Color newColor, float transitionTime)
        {
            float timer = 0.0f;
            Color startColor = targetGraphic.color;

            while (timer < transitionTime)
            {
                timer += Time.unscaledDeltaTime;

                yield return null;

                targetGraphic.color = Color.Lerp(startColor, newColor, timer / transitionTime);
            }
        }

        [Serializable]
        public class UIClickEvent : UnityEvent {}

        [Serializable]
        public class ColorBlock
        {
            public Color normalColor;
            public Color highlightedColor;
            public Color pressedColor;
            public Color disabledColor;
            public Color selectedColor;

            public ColorBlock()
            {
                normalColor = new Color(0.81f, 0.81f, 0.81f, 1.0f);
                highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1.0f);
                pressedColor = new Color(0.78f, 0.78f, 0.78f, 1.0f);
                disabledColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                selectedColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }

#if UNITY_EDITOR

            [CustomPropertyDrawer(typeof(ColorBlock))]
            public class ColorBlockDrawer : PropertyDrawer
            {
                public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                {
                    return EditorGUIUtility.singleLineHeight * (EditorGUIUtility.wideMode ? 1 : 2) * 5 + 16;
                }

                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    // Find the SerializedProperties by name
                    SerializedProperty normalColorProperty = property.FindPropertyRelative(nameof(normalColor));
                    SerializedProperty highlightedColorProperty = property.FindPropertyRelative(nameof(highlightedColor));
                    SerializedProperty pressedColorProperty = property.FindPropertyRelative(nameof(pressedColor));
                    SerializedProperty disabledColorProperty = property.FindPropertyRelative(nameof(disabledColor));
                    SerializedProperty selectedColorProperty = property.FindPropertyRelative(nameof(selectedColor));

                    // Using BeginProperty / EndProperty on the parent property means that
                    // prefab override logic works on the entire property.
                    float addY = 20;
                    EditorGUI.BeginProperty(position, label, property);

                    EditorGUI.indentLevel++;
                    Rect rect = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(normalColorProperty));
                    normalColorProperty.colorValue = EditorGUI.ColorField(rect, label, normalColorProperty.colorValue, true, true, false);

                    label.text = highlightedColorProperty.displayName;
                    rect.y += addY;
                    highlightedColorProperty.colorValue =
                        EditorGUI.ColorField(rect, label, highlightedColorProperty.colorValue, true, true, false);

                    label.text = pressedColorProperty.displayName;
                    rect.y += addY;
                    pressedColorProperty.colorValue = EditorGUI.ColorField(rect, label, pressedColorProperty.colorValue, true, true, false);

                    label.text = disabledColorProperty.displayName;
                    rect.y += addY;
                    disabledColorProperty.colorValue =
                        EditorGUI.ColorField(rect, label, disabledColorProperty.colorValue, true, true, false);

                    label.text = selectedColorProperty.displayName;
                    rect.y += addY;
                    selectedColorProperty.colorValue =
                        EditorGUI.ColorField(rect, label, selectedColorProperty.colorValue, true, true, false);
                    EditorGUI.indentLevel--;

                    EditorGUI.EndProperty();
                }
            }
#endif
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (isEnabled)
            {
                targetGraphic.color = colorBlock.normalColor;
                _buttonStatus = ButtonStatus.Normal;
            }
            else
            {
                targetGraphic.color = colorBlock.disabledColor;
                _buttonStatus = ButtonStatus.Disabled;
            }
        }

        private void Reset()
        {
            targetGraphic = GetComponentInChildren<TextMeshProUGUI>();
        }
#endif
    }
}