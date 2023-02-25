using System;
using MyBox;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeepDreams.UI
{
    [RequireComponent(typeof(Touchable))]
    public class CanvasGroupBlocker : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private bool shouldDim;

        [ConditionalField(nameof(shouldDim), false, true)] [Range(0.0f, 3.0f)] [SerializeField] private float dimTime = 0.05f;
        [SerializeField] private float startAlpha = 1.0f;
        [SerializeField] private float endAlpha = 0.75f;
        [SerializeField] private bool enableOnAwake;

        private float _alphaVelocity;
        private float _targetAlpha;

        private CanvasGroup _canvasGroup;
        private Graphic _touchable;

        public event Action OnClicked;

        private void Awake()
        {
            _touchable = GetComponent<Touchable>();
            _canvasGroup = transform.parent.gameObject.GetOrAddComponent<CanvasGroup>();
            OnToggle(enableOnAwake);
        }

        // Update is called once per frame
        private void Update()
        {
            _canvasGroup.alpha = Mathf.SmoothDamp(_canvasGroup.alpha, _targetAlpha, ref _alphaVelocity, dimTime, Mathf.Infinity,
                Time.unscaledDeltaTime);
        }

        private void Block()
        {
            _targetAlpha = endAlpha;
            _touchable.raycastTarget = true;
        }

        private void Unblock()
        {
            _targetAlpha = startAlpha;
            _touchable.raycastTarget = false;
        }

        public void OnToggle(bool isSelected)
        {
            if (isSelected) Block();
            else Unblock();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnToggle(false);
            OnClicked?.Invoke();
        }
    }
}