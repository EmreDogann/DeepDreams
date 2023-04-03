﻿using DeepDreams.ScriptableObjects.Events.UnityEvents;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepDreams.Player
{
    public class PlayerMouseLook : MonoBehaviour
    {
        [SerializeField] private Vector2 lookSpeed = new Vector2(6.0f, 6.0f);
        [SerializeField] private float lookDownLimit;
        [SerializeField] private float lookUpLimit;

        [Space]
        [SerializeField] [Range(0.0f, 1.0f)] private float mouseDamping = 0.03f;

        [Space]
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private bool useEditorRotation;

        private Vector2 _lookDirection;
        private Vector2 _currentMouseDelta;
        private Vector2 _currentMouseDeltaVelocity;

        private bool _isPaused;
        private BoolEventListener _onGamePausedEvent;

        private void Awake()
        {
            _onGamePausedEvent = GetComponent<BoolEventListener>();

            if (useEditorRotation) _lookDirection = new Vector2(transform.localEulerAngles.x, transform.localEulerAngles.y);
        }

        private void OnEnable()
        {
            _onGamePausedEvent.Response.AddListener(OnGamePause);
        }

        private void OnDisable()
        {
            _onGamePausedEvent.Response.RemoveListener(OnGamePause);
        }

        private void Update()
        {
            if (_isPaused) return;

            CameraLook();
        }

        private void CameraLook()
        {
            transform.localEulerAngles = Vector3.up * _lookDirection.y;
            cameraHolder.localEulerAngles = Vector3.right * _lookDirection.x;
        }

        private void OnLook(InputValue value)
        {
            if (_isPaused) return;

            Vector2 targetMouseDelta = value.Get<Vector2>();
            _currentMouseDelta = Vector2.SmoothDamp(_currentMouseDelta, targetMouseDelta, ref _currentMouseDeltaVelocity, mouseDamping);

            _lookDirection.y += _currentMouseDelta.x * lookSpeed.x * 0.01f;
            _lookDirection.x -= _currentMouseDelta.y * lookSpeed.y * 0.01f;
            _lookDirection.x = Mathf.Clamp(_lookDirection.x, lookDownLimit, lookUpLimit);
        }

        private void OnGamePause(bool isPaused)
        {
            _isPaused = isPaused;
        }
    }
}