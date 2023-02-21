using UnityEngine;

namespace DeepDreams.Utils
{
    public class Utils
    {
        public static void ResetCurve(AnimationCurve curve) {
            for (int i = curve.keys.Length - 1; i >= 0; i--) curve.RemoveKey(i);
        }

        public static void SetSineCurve(AnimationCurve curve) {
            ResetCurve(curve);

            float maxValue = 2 * Mathf.PI;
            curve.postWrapMode = WrapMode.Loop;
            curve.AddKey(new Keyframe(0, 0, maxValue, maxValue)); // 0, sin(0), sin'(0) = cos(0), sin'(0) = cos(0)
            curve.AddKey(new Keyframe(0.5f * Mathf.PI / maxValue, 1, 0, 0));
            curve.AddKey(new Keyframe(Mathf.PI / maxValue, 0, -maxValue, -maxValue));
            curve.AddKey(new Keyframe(1.5f * Mathf.PI / maxValue, -1, 0, 0));
            curve.AddKey(new Keyframe(2f * Mathf.PI / maxValue, 0, maxValue, maxValue));
        }

        public static void SetCosineCurve(AnimationCurve curve) {
            ResetCurve(curve);

            float maxValue = 2 * Mathf.PI;
            curve.postWrapMode = WrapMode.Loop;
            curve.AddKey(new Keyframe(0, 1, 0, 0));
            curve.AddKey(new Keyframe(0.5f * Mathf.PI / maxValue, 0, -maxValue, -maxValue));
            curve.AddKey(new Keyframe(Mathf.PI / maxValue, -1, 0, 0));
            curve.AddKey(new Keyframe(1.5f * Mathf.PI / maxValue, 0, maxValue, maxValue));
            curve.AddKey(new Keyframe(2f * Mathf.PI / maxValue, 1, 0, 0));
        }
    }
}