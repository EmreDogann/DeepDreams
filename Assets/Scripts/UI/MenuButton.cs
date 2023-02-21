using System;
using System.Collections;
using DeepDreams.Audio;
using MyBox;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeepDreams.UI
{
    public class MenuButton : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler,
        IPointerDownHandler
    {
        [Separator("General")]
        [OverrideLabel("Interactable")] public bool isEnabled = true;
        [OverrideLabel("Toggleable")] public bool isToggleable;

        public Graphic targetGraphic;

        public ColorBlock colorBlock = new ColorBlock();

        [Range(0.0f, 5.0f)] public float transitionTime = 0.1f;

        [Serializable]
        public class UIClickEvent : UnityEvent {}
        [Space]
        public UIClickEvent onClickEvent;

        private bool _isSelected;
        private bool _isHighlighted;
        private ButtonStatus _buttonStatus = ButtonStatus.Normal;

        private void Awake()
        {
            if (targetGraphic == null) targetGraphic = GetComponentInChildren<TextMeshProUGUI>();

            if (!isEnabled)
            {
                targetGraphic.color = colorBlock.disabledColor;
                _buttonStatus = ButtonStatus.Disabled;
            }
            else targetGraphic.color = colorBlock.normalColor;
        }

        public bool IsSelected()
        {
            return _buttonStatus == ButtonStatus.Selected;
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

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!isEnabled || IsSelected()) return;
            _isHighlighted = true;
            _buttonStatus = ButtonStatus.Highlighted;

            StartCoroutine(TransitionColor(colorBlock.highlightedColor, transitionTime));
            AudioManager.instance.PlayOneShot(FMODEvents.instance.UI_Hover, Vector3.zero);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (!isEnabled || IsSelected()) return;
            _isHighlighted = false;
            _buttonStatus = ButtonStatus.Normal;

            StartCoroutine(TransitionColor(colorBlock.normalColor, transitionTime));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isEnabled) return;
            _buttonStatus = ButtonStatus.Pressed;
            StartCoroutine(TransitionColor(colorBlock.pressedColor, transitionTime));
            AudioManager.instance.PlayOneShot(FMODEvents.instance.UI_Click, Vector3.zero);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!isEnabled) return;
            if (isToggleable) _isSelected = !_isSelected;

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

            onClickEvent?.Invoke();
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

        public enum ButtonStatus
        {
            Normal,
            Disabled,
            Highlighted,
            Pressed,
            Selected
        }

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
    }
}