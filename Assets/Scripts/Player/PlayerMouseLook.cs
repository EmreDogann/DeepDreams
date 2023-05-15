using System;
using System.Collections;
using DeepDreams.SaveLoad;
using DeepDreams.SaveLoad.Data;
using DeepDreams.ScriptableObjects.Events.UnityEvents;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepDreams.Player
{
    public class PlayerMouseLook : MonoBehaviour, ISaveable<GameData>
    {
        [SerializeField] private Vector2 lookSpeed = new Vector2(6.0f, 6.0f);
        [SerializeField] private float lookDownLimit;
        [SerializeField] private float lookUpLimit;

        [Space]
        [SerializeField] [Range(0.0f, 1.0f)] private float mouseDamping = 0.03f;

        [Space]
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private bool useEditorRotation;

        private Vector2 _currentLookSensitivity;
        private Vector2 _lookDirection;
        private Vector2 _currentMouseDelta;
        private Vector2 _currentMouseDeltaVelocity;

        private Coroutine _changeSensitivityCoroutine;

        private bool _isPaused;
        private BoolEventListener _onGamePausedEvent;

        public static Action<float, float> LookSensitivityMultiply;

        private void Awake()
        {
            _onGamePausedEvent = GetComponent<BoolEventListener>();
        }

        private void Start()
        {
            if (useEditorRotation)
            {
                _lookDirection = new Vector2(cameraHolder.localEulerAngles.x, transform.localEulerAngles.y);
            }

            _currentLookSensitivity = lookSpeed;
        }

        private void OnEnable()
        {
            _onGamePausedEvent.Response.AddListener(OnGamePause);
            LookSensitivityMultiply += ChangeSensitivity;
        }

        private void OnDisable()
        {
            _onGamePausedEvent.Response.RemoveListener(OnGamePause);
            LookSensitivityMultiply -= ChangeSensitivity;
        }

        private void ChangeSensitivity(float multiplier, float duration)
        {
            if (_changeSensitivityCoroutine != null)
            {
                StopCoroutine(_changeSensitivityCoroutine);
            }

            _changeSensitivityCoroutine =
                StartCoroutine(LerpSensitivity(_currentLookSensitivity, lookSpeed * multiplier, duration));
        }

        private IEnumerator LerpSensitivity(Vector2 start, Vector2 end, float duration)
        {
            float timeElapsed = 0.0f;

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;
                _currentLookSensitivity = Vector2.Lerp(start, end, t);
                timeElapsed += Time.deltaTime;

                yield return null;
            }

            _currentLookSensitivity = end;
        }

        private void Update()
        {
            if (_isPaused)
            {
                return;
            }

            CameraLook();
        }

        private void CameraLook()
        {
            transform.localEulerAngles = Vector3.up * _lookDirection.y;
            cameraHolder.localEulerAngles = Vector3.right * _lookDirection.x;
        }

        private void OnLook(InputValue value)
        {
            if (_isPaused)
            {
                return;
            }

            Vector2 targetMouseDelta = value.Get<Vector2>();
            _currentMouseDelta = Vector2.SmoothDamp(_currentMouseDelta, targetMouseDelta,
                ref _currentMouseDeltaVelocity, mouseDamping);

            _lookDirection.y += _currentMouseDelta.x * _currentLookSensitivity.x * 0.01f;
            _lookDirection.y = WrapAngle(_lookDirection.y, 0.0f, 360.0f);

            _lookDirection.x -= _currentMouseDelta.y * _currentLookSensitivity.y * 0.01f;
            _lookDirection.x = Clamp(_lookDirection.x, lookUpLimit, lookDownLimit);
        }

        // From: https://stackoverflow.com/a/2021986/10439539
        // Normalizes any number to an arbitrary range by assuming the range wraps around when going below min or above max.
        private float WrapAngle(float value, float start, float end)
        {
            float range = end - start;
            float offsetValue = value - start; // value relative to 0

            return offsetValue - Mathf.Floor(offsetValue / range) * range + start;
            // + start to reset back to start of original range
        }

        // From: http://answers.unity.com/comments/1406762/view.html
        // Expects angle in the range 0 to 360
        // Expects min and max in the range -180 to 180
        // Returns the clamped angle in the range 0 to 360
        private float Clamp(float angle, float min, float max)
        {
            if (angle > 180.0f) // remap 0 - 360 --> -180 - 180
            {
                angle -= 360.0f;
            }

            angle = Mathf.Clamp(angle, min, max);

            if (angle < 0.0f) // map back to 0 - 360
            {
                angle += 360.0f;
            }

            return angle;
        }

        private void OnGamePause(bool isPaused)
        {
            _isPaused = isPaused;
        }

        public void SaveData(GameData saveData)
        {
            saveData.playerOrientation = Quaternion.Euler(cameraHolder.localEulerAngles.x,
                transform.localEulerAngles.y, transform.localEulerAngles.z);
        }

        public void LoadData(GameData saveData)
        {
            Vector3 orientation = saveData.playerOrientation.eulerAngles;
            transform.rotation =
                Quaternion.Euler(transform.localEulerAngles.x, orientation.y, transform.localEulerAngles.z);

            cameraHolder.rotation = Quaternion.Euler(orientation.x,
                cameraHolder.localEulerAngles.y, cameraHolder.localEulerAngles.z);
        }
    }
}