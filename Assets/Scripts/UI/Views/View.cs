using System.Collections;
using DeepDreams.UI.Views.Transitions;
using UnityEngine;

namespace DeepDreams.UI.Views
{
    [RequireComponent(typeof(CanvasGroup))]
    public class View : MonoBehaviour
    {
        [SerializeField] private MenuTransitionFactory menuTransitionFactory;
        protected Coroutine _coroutine;

        protected bool _isActive;
        private MenuTransition _menuTransition;
        public CanvasGroup CanvasGroup { get; private set; }

        public virtual void Initialize()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            CanvasGroup.blocksRaycasts = false;

            _menuTransition = menuTransitionFactory.CreateTransition();
            _menuTransition.Initialize(this);
        }

        public virtual void Open()
        {
            _isActive = true;
            gameObject.SetActive(_isActive);

            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }

            _coroutine = StartCoroutine(Show());
        }

        public virtual void Close()
        {
            if (!_isActive)
            {
                return;
            }

            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }

            _coroutine = StartCoroutine(Hide());
        }

        protected virtual IEnumerator Show()
        {
            yield return _menuTransition.Show(this);
            CanvasGroup.blocksRaycasts = true;
        }

        protected virtual IEnumerator Hide()
        {
            CanvasGroup.blocksRaycasts = false;
            yield return _menuTransition.Hide(this);
            _isActive = false;
            gameObject.SetActive(_isActive);
        }
    }
}