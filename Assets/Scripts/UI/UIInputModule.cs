using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace DeepDreams.UI
{
    public class UIInputModule : MonoBehaviour
    {
        public static Action<bool> OnCancelEvent;
        private InputAction _cancel;
        private InputSystemUIInputModule _uiInputModule;

        private void Awake()
        {
            _uiInputModule = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InputSystemUIInputModule>();

            if (_uiInputModule != null)
            {
                _cancel = _uiInputModule.cancel.action;
            }
        }

        private void OnEnable()
        {
            _cancel.started += OnCancel;
        }

        private void OnDisable()
        {
            _cancel.started -= OnCancel;
        }

        public void OnCancel(InputAction.CallbackContext ctx)
        {
            View view = UIManager.instance.GetCurrentView();
            OnCancelEvent?.Invoke(view);

            if (!view)
            {
                UIManager.instance.Show<PauseMenuView>();
            }
            else
            {
                UIManager.instance.Back();
            }
        }
    }
}