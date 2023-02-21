using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace DeepDreams.UI
{
    public class ViewInputModule : MonoBehaviour
    {
        private InputSystemUIInputModule _uiInputModule;
        private InputAction _cancel;

        public static Action<bool> OnCancelEvent;

        private void Awake()
        {
            _uiInputModule = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InputSystemUIInputModule>();
            _cancel = _uiInputModule.cancel.action;
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
            View view = ViewManager.instance.GetCurrentView();

            if (!view) ViewManager.instance.Show<PauseMenuView>();
            else ViewManager.instance.Back();

            OnCancelEvent?.Invoke(view);
        }
    }
}