using DeepDreams.ThirdPartyAssets.GG_Camera_Shake.Runtime;
using MyBox;
using UnityEngine;

namespace DeepDreams.Player.Camera
{
    [CreateAssetMenu(menuName = "Camera/Camera Curve Shake", fileName = "New Camera Curve Shake", order = 0)]
    public class CameraCurveShake : ScriptableObject, ICameraShake
    {
        [Separator("XY Motion")]
        [SerializeField] private CameraCurveParams motionCurveParams;
        [Separator("Tilt")]
        [SerializeField] private CameraCurveParams tiltCurveParams;

        private float _time;
        private Vector3 motionVelocity;
        private Vector3 tiltVelocity;

        private Displacement _currentDisplacement;
        // ReSharper disable once ConvertToAutoProperty
        public Displacement CurrentDisplacement => _currentDisplacement;
        public bool IsFinished { get; private set; }

        public void Invert(int sign)
        {
            motionCurveParams.Invert(sign);
            tiltCurveParams.Invert(sign);
        }

        public void Initialize(Vector3 cameraPosition, Quaternion cameraRotation)
        {
            motionCurveParams.Init();
            tiltCurveParams.Init();

            _time = 0.0f;
            _currentDisplacement = Displacement.Zero;
            IsFinished = false;
        }

        public void UpdateShake(float deltaTime, Vector3 cameraPosition, Quaternion cameraRotation)
        {
            _time += deltaTime;

            if (motionCurveParams.enable && !motionCurveParams.IsFinished(_time)) _currentDisplacement.position += EvaluateMotion(_time);
            if (tiltCurveParams.enable && !tiltCurveParams.IsFinished(_time)) _currentDisplacement.eulerAngles += EvaluateTilt(_time);

            if (motionCurveParams.IsFinished(_time) && tiltCurveParams.IsFinished(_time)) IsFinished = true;

            // ResetPosition();
        }

        private Vector3 EvaluateMotion(float time)
        {
            Vector3 pos = Vector3.zero;

            pos.x = motionCurveParams.xCurve.Evaluate(time) * motionCurveParams.invert * motionCurveParams.xCurveAmplitude -
                    _currentDisplacement.position.x;
            pos.y = motionCurveParams.yCurve.Evaluate(time) * motionCurveParams.invert * motionCurveParams.yCurveAmplitude -
                    _currentDisplacement.position.y;
            pos.z = motionCurveParams.zCurve.Evaluate(time) * motionCurveParams.invert * motionCurveParams.zCurveAmplitude -
                    _currentDisplacement.position.z;


            return pos;
        }

        private Vector3 EvaluateTilt(float time)
        {
            Vector3 rot = Vector3.zero;

            rot.x = tiltCurveParams.xCurve.Evaluate(time * tiltCurveParams.xCurveTimescale) * tiltCurveParams.invert *
                    tiltCurveParams.xCurveAmplitude -
                    _currentDisplacement.eulerAngles.x;
            rot.y = tiltCurveParams.yCurve.Evaluate(time * tiltCurveParams.yCurveTimescale) * tiltCurveParams.invert *
                    tiltCurveParams.yCurveAmplitude -
                    _currentDisplacement.eulerAngles.y;
            rot.z = tiltCurveParams.zCurve.Evaluate(time * tiltCurveParams.zCurveTimescale) * tiltCurveParams.invert *
                    tiltCurveParams.zCurveAmplitude -
                    _currentDisplacement.eulerAngles.z;

            return rot;
        }

        private void ResetPosition()
        {
            if ((_currentDisplacement.position - Vector3.zero).magnitude <= 0.001f) _currentDisplacement.position = Vector3.zero;
            if ((_currentDisplacement.eulerAngles - Vector3.zero).magnitude <= 0.001f) _currentDisplacement.eulerAngles = Vector3.zero;

            _currentDisplacement.position =
                Vector3.SmoothDamp(_currentDisplacement.position, Vector3.zero, ref motionVelocity, motionCurveParams.bobSmoothing);
            _currentDisplacement.eulerAngles = Vector3.SmoothDamp(_currentDisplacement.eulerAngles, Vector3.zero, ref tiltVelocity,
                tiltCurveParams.bobSmoothing);
        }
    }
}