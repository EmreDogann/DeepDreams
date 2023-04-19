using System;
using MyBox;
using UnityEngine;

namespace DeepDreams.Player.PlayerCamera
{
    [Serializable]
    public class CameraCurveParams
    {
        public bool enable = true;

        public float bobSmoothing = 0.1f;

        public AnimationCurve xCurve;
        public AnimationCurve yCurve;
        public AnimationCurve zCurve;

        [Space]
        [OverrideLabel("X Amplitude")] public float xCurveAmplitude = 0.015f;
        [OverrideLabel("Y Amplitude")] public float yCurveAmplitude = 0.015f;
        [OverrideLabel("Z Amplitude")] public float zCurveAmplitude = 0.015f;

        [Space]
        [OverrideLabel("X Timescale")] public float xCurveTimescale = 1.0f;
        [OverrideLabel("Y Timescale")] public float yCurveTimescale = 1.0f;
        [OverrideLabel("Z Timescale")] public float zCurveTimescale = 1.0f;

        [HideInInspector] public int invert = 1;

        private float _xCurveEndTime;
        private float _yCurveEndTime;
        private float _zCurveEndTime;

        public void Init()
        {
            CheckLastKeyFrame(xCurve, ref _xCurveEndTime);
            CheckLastKeyFrame(yCurve, ref _yCurveEndTime);
            CheckLastKeyFrame(zCurve, ref _zCurveEndTime);
        }

        public void Invert()
        {
            invert *= -1;
        }

        public void Invert(int sign)
        {
            invert = sign;
        }

        public bool IsFinished(float time)
        {
            if (time > _xCurveEndTime * xCurveTimescale && time > _yCurveEndTime * yCurveTimescale &&
                time > _zCurveEndTime * zCurveTimescale) return true;

            return false;
        }

        private void CheckLastKeyFrame(AnimationCurve curve, ref float lastKeyframeTime)
        {
            // Check if there are no keyframes (i.e. no curve)
            if (curve.length == 0) return;

            lastKeyframeTime = curve[curve.length - 1].time;
        }
    }
}