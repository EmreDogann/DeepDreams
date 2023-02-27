using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeepDreams.UI.Components
{
    [Serializable]
    public class ScrollRectNoDrag : ScrollRect
    {
        [SerializeField] private float optionSpacing;
        [SerializeField] private int numberOfOptions;
        [SerializeField] private float optionHeight;
        [SerializeField] private float scrollSensitivityStepAmount;

        private Coroutine _coroutine;

        protected override void Awake()
        {
            base.Awake();

            Rect newRect = viewRect.rect;
            newRect.height = optionHeight * numberOfOptions + optionSpacing * (numberOfOptions - 1);
            viewRect.rect.Set(newRect.x, newRect.y, newRect.width, newRect.height);
        }

        public override void OnBeginDrag(PointerEventData eventData) {}
        public override void OnDrag(PointerEventData eventData) {}
        public override void OnEndDrag(PointerEventData eventData) {}

        public void OnCategoryChanged()
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(SetScrolling());
        }

        private IEnumerator SetScrolling()
        {
            yield return new WaitForEndOfFrame();

            float contentHeight = content.rect.height;
            float itemHeight = optionHeight + optionSpacing;
            verticalScrollbar.numberOfSteps = 1 + Mathf.Max(0, (int)((contentHeight - viewRect.rect.height) / itemHeight));
            scrollSensitivity = itemHeight / scrollSensitivityStepAmount;
        }
    }
}