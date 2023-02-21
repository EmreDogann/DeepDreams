using System;
using System.Collections;
using DeepDreams.Utils;
using UnityEngine;

namespace DeepDreams.UI
{
    [Serializable]
    public class FadeMenuTransition : MenuTransition
    {
        private CanvasGroup _canvasGroup;
        [SerializeField] private float openDuration = 0.1f;
        [SerializeField] private float closeDuration = 0.1f;
        [SerializeField] private Easing openEasing = Easing.Linear;
        [SerializeField] private Easing closeEasing = Easing.Linear;

        public override void Initialize(View view)
        {
            _canvasGroup = view.CanvasGroup;
            view.gameObject.SetActive(false);
            _canvasGroup.alpha = 0.0f;
        }

        private IEnumerator Fade(float start, float end, float duration, Func<float, float> ease)
        {
            float current = _canvasGroup.alpha;
            float elapsedTime = Mathf.InverseLerp(start, end, current) * duration;

            while (elapsedTime < duration)
            {
                _canvasGroup.alpha = Mathf.Lerp(start, end, ease(elapsedTime / duration));
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            _canvasGroup.alpha = end;
        }

        public override IEnumerator Show(View view)
        {
            yield return Fade(_canvasGroup.alpha, 1.0f, openDuration, openEasing.GetFunction());
        }

        public override IEnumerator Hide(View view)
        {
            yield return Fade(_canvasGroup.alpha, 0.0f, closeDuration, closeEasing.GetFunction());
        }
    }
}