using System;
using System.Collections.Generic;
using DeepDreams.ScriptableObjects.Events;
using UnityEngine;

namespace DeepDreams.UI.Views
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private View baseView; // Quick hacky workaround
        [SerializeField] private View inGameView;

        [SerializeField] private View startingView;
        [SerializeField] private BoolEventChannelSO onGamePauseSOEvent;
        private readonly Stack<View> _history = new Stack<View>();

        private View _currentView;
        private View[] _views;

        public Action<bool> OnGamePause;
        public static UIManager instance { get; private set; }

        private void Awake()
        {
            instance = this;
            HideCursor();
        }

        private void Start()
        {
            _views = Resources.FindObjectsOfTypeAll(typeof(View)) as View[];

            if (_views == null)
            {
                Debug.LogWarning("Warning - View Manager: No views in the scene have been found!");
                return;
            }

            for (int i = 0; i < _views.Length; i++)
            {
                _views[i].Initialize();
            }

            if (startingView != null)
            {
                Show(startingView);
                ShowCursor();
            }
            else
            {
                instance.inGameView.Open();
            }
        }

        private void ShowCursor()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        private void HideCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private bool IsOnlyView()
        {
            return instance._history.Count == 0;
        }

        public View GetCurrentView()
        {
            return instance._currentView;
        }

        public T GetView<T>() where T : View
        {
            for (int i = 0; i < instance._views.Length; i++)
            {
                if (instance._views[i] is T tView)
                {
                    return tView;
                }
            }

            return null;
        }


        public void Show<T>(bool remember = true) where T : View
        {
            for (int i = 0; i < instance._views.Length; i++)
            {
                if (instance._views[i] is not T)
                {
                    continue;
                }

                if (instance._currentView != null)
                {
                    if (remember)
                    {
                        instance._history.Push(instance._currentView);
                    }

                    instance._currentView.Close();
                }
                else
                {
                    instance.inGameView.Close();
                    instance.baseView.Open();
                    Time.timeScale = 0.0f;
                    onGamePauseSOEvent.Raise(true);
                }

                instance._views[i].Open();
                instance._currentView = instance._views[i];
            }

            ShowCursor();
        }

        public void Show(View view, bool remember = true)
        {
            if (instance._currentView != null)
            {
                if (remember)
                {
                    instance._history.Push(instance._currentView);
                }

                instance._currentView.Close();
            }
            else
            {
                instance.inGameView.Close();
                instance.baseView.Open();
                Time.timeScale = 0.0f;
                onGamePauseSOEvent.Raise(true);
            }

            view.Open();
            instance._currentView = view;

            ShowCursor();
        }

        public void Back()
        {
            if (IsOnlyView())
            {
                instance._currentView.Close();
                instance.baseView.Close();
                instance.inGameView.Open();
                instance._currentView = null;

                Time.timeScale = 1.0f;
                onGamePauseSOEvent.Raise(false);

                HideCursor();
                return;
            }

            Show(instance._history.Pop(), false);
        }
    }
}