using DeepDreams.Audio;
using DeepDreams.ScriptableObjects.Audio;
using MyBox;
using UnityEngine;

namespace DeepDreams.Player.Camera
{
    public class CameraHeadBob : MonoBehaviour
    {
        [Separator("General")]
        [SerializeField] private bool enable = true;

        [SerializeField] private float headBobSmoothing = 0.1f;

        [SerializeField] private Transform camera;
        [SerializeField] private PlayerMotor playerController;

        [Separator("Audio")]
        [SerializeField] private AudioReference footstepWalk;
        [SerializeField] private AudioReference footstepRun;

        [Separator("Bob Motion")]
        [SerializeField] private AnimationCurve yMotion;
        [SerializeField] private AnimationCurve xMotion;
        [OverrideLabel("Y Amplitude")] [SerializeField] private float yMotionAmplitude = 0.015f;
        [OverrideLabel("X Amplitude")] [SerializeField] private float xMotionAmplitude = 0.015f;
        [OverrideLabel("Running Multiplier")] [SerializeField] private float motionRunMultiplier = 0.015f;

        [Separator("Bob Tilt")]
        [SerializeField] private float xTilt;
        [SerializeField] private AnimationCurve zWalkingTilt;
        [SerializeField] private AnimationCurve zRunningTilt;
        [OverrideLabel("Z Tilt Amplitude")] [SerializeField] private float zTiltAmplitude = 0.015f;
        [OverrideLabel("Running Multiplier")] [SerializeField] private float tiltRunMultiplier = 0.015f;

        [Separator("Synchronization")]
        [Tooltip("The time (x-axis value) on the Y Motion curve which represents when the footstep occurs.")]
        [SerializeField] private float stepTime;

        [Separator("Stabilization")]
        [SerializeField] private bool enableStabilization = true;
        [ConditionalField("enableStabilization", false, true)] [SerializeField] private float stabilizationDistance = 5.0f;

        [Separator("Debugging")]
        [SerializeField] private bool enableDebugging;
        private float _motionAmplitudeMultiplier;
        private float _motionAmplitudeMultiplierVelocity;

        private bool _onStepCalled;

        private Vector3 _startPos;
        private Quaternion _startRot;
        private int _stepCount;
        private float _targetAmplitudeMultiplier;
        private float _tiltAmplitudeMultiplier;
        private float _tiltAmplitudeMultiplierVelocity;

        private float _walkingTime;

        private float _xTiltTarget;
        private float _xTiltVelocity;

        private void Start()
        {
            _startPos = camera.localPosition;
            _startRot = camera.localRotation;

            if (!enableDebugging)
            {
                return;
            }

            DebugGUI.SetGraphProperties("bobbingX", "X", -0.05f, 0.05f, 0, new Color(1, 0, 0), false);
            DebugGUI.SetGraphProperties("bobbingY", "Y", -0.05f, 0.05f, 0, new Color(0, 1, 0), false);
            DebugGUI.SetGraphProperties("bobbingZTilt", "ZTilt", -0.25f, 0.25f, 0, new Color(0, 1, 1), false);

            DebugGUI.SetGraphProperties("bobbingXEvent", "EventX", -0.05f, 0.05f, 0, new Color(1, 1, 0), false);
            DebugGUI.SetGraphProperties("bobbingYEvent", "EventY", -0.05f, 0.05f, 0, new Color(1, 0, 1), false);
            DebugGUI.SetGraphProperties("bobbingZTiltEvent", "EventZTilt", -0.25f, 0.25f, 0, new Color(1, 1, 1), false);
        }

        private void Update()
        {
            if (!enable)
            {
                return;
            }

            if (enableDebugging)
            {
                if (!_onStepCalled)
                {
                    DebugGUI.Graph("bobbingXEvent", 0);
                    DebugGUI.Graph("bobbingYEvent", 0);
                    DebugGUI.Graph("bobbingZTiltEvent", 0);
                }

                _onStepCalled = false;

                DebugGUI.Graph("bobbingX", camera.localPosition.x);
                DebugGUI.Graph("bobbingY", camera.localPosition.y);
                DebugGUI.Graph("bobbingZTilt", WrapAngle(camera.localEulerAngles.z));
            }

            CheckMotion();
            ResetPosition();
        }

        private void OnEnable()
        {
            playerController.OnStride += OnStep;
        }

        private void OnDisable()
        {
            playerController.OnStride -= OnStep;
        }

        private bool IsMoving()
        {
            return playerController.IsMoving && playerController.IsGrounded;
        }

        private bool IsRunning()
        {
            return playerController.IsRunning && !playerController.IsCrouching && playerController.IsMovingForward &&
                   playerController.IsGrounded;
        }

        private float WrapAngle(float angle)
        {
            angle %= 360;

            if (angle > 180)
            {
                return angle - 360;
            }

            return angle;
        }

        private void OnStep()
        {
            if (IsRunning())
            {
                AudioManager.instance.PlayOneShot(footstepRun);
            }
            else
            {
                AudioManager.instance.PlayOneShot(footstepWalk);
            }

            _stepCount++;

            if (!enableDebugging)
            {
                return;
            }

            _onStepCalled = true;
            DebugGUI.GraphEvent("bobbingXEvent", camera.localPosition.x);
            DebugGUI.GraphEvent("bobbingYEvent", camera.localPosition.y);
            DebugGUI.GraphEvent("bobbingZTiltEvent", WrapAngle(camera.localEulerAngles.z));
        }

        private void CheckMotion()
        {
            if (!IsMoving())
            {
                _stepCount = 0;
                _motionAmplitudeMultiplier = 0.0f;
                _tiltAmplitudeMultiplier = 0.0f;

                return;
            }

            _walkingTime = playerController.StrideDistance / playerController.PlayerStride;

            camera.localPosition += FootStepMotion(_walkingTime);

            if (enableStabilization)
            {
                camera.LookAt(camera.transform.position + camera.forward * stabilizationDistance);
            }

            camera.localRotation *= Quaternion.Euler(FootStepTilt(_walkingTime));
        }

        private Vector3 FootStepMotion(float time)
        {
            Vector3 pos = Vector3.zero;

            _targetAmplitudeMultiplier = 1.0f;

            if (IsRunning())
            {
                _targetAmplitudeMultiplier = motionRunMultiplier;
            }

            _motionAmplitudeMultiplier = Mathf.SmoothDamp(_motionAmplitudeMultiplier, _targetAmplitudeMultiplier,
                ref _motionAmplitudeMultiplierVelocity, 0.1f);

            pos.y = yMotion.Evaluate(time + stepTime) * yMotionAmplitude * _motionAmplitudeMultiplier - camera.localPosition.y;
            pos.x = xMotion.Evaluate((time + _stepCount) * 0.5f) * xMotionAmplitude * _motionAmplitudeMultiplier - camera.localPosition.x;

            return pos;
        }

        private Vector3 FootStepTilt(float time)
        {
            Vector3 rot = Vector3.zero;
            AnimationCurve curve = zWalkingTilt;

            _targetAmplitudeMultiplier = 1.0f;
            _xTiltTarget = 0.0f;

            if (IsRunning())
            {
                _targetAmplitudeMultiplier = tiltRunMultiplier;
                curve = zRunningTilt;
            }
            else
            {
                if (IsMoving())
                {
                    _xTiltTarget = xTilt;
                }
            }

            _tiltAmplitudeMultiplier = Mathf.SmoothDamp(_tiltAmplitudeMultiplier, _targetAmplitudeMultiplier,
                ref _tiltAmplitudeMultiplierVelocity, 0.1f);

            rot.z = curve.Evaluate((time + _stepCount) * 0.5f) * zTiltAmplitude * _tiltAmplitudeMultiplier - camera.localEulerAngles.z;

            rot.x = Mathf.SmoothDamp(camera.localEulerAngles.x, _xTiltTarget, ref _xTiltVelocity, 0.2f) - camera.localEulerAngles.x;

            return rot;
        }

        private void ResetPosition()
        {
            if ((camera.localPosition - _startPos).magnitude <= 0.00001f)
            {
                camera.localPosition = _startPos;
            }

            if (camera.localPosition == _startPos && camera.localRotation == _startRot)
            {
                return;
            }

            camera.localPosition = Vector3.Lerp(camera.localPosition, _startPos, headBobSmoothing);
            camera.localRotation = Quaternion.Lerp(camera.localRotation, _startRot, headBobSmoothing);
        }
    }
}