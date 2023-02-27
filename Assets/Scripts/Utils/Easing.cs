using System;
using UnityEngine;

namespace DeepDreams.Utils
{
    // From: https://github.com/peartreegames/evt-menus/blob/main/Runtime/Transitions/Easing.cs
    // Based on easings.net: https://github.com/ai/easings.net
    public enum Easing
    {
        Linear,
        QuadraticIn,
        QuadraticOut,
        QuadraticInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
        QuarticIn,
        QuarticOut,
        QuarticInOut,
        QuinticIn,
        QuinticOut,
        QuinticInOut,
        SinusoidalIn,
        SinusoidalOut,
        SinusoidalInOut,
        ExponentialIn,
        ExponentialOut,
        ExponentialInOut,
        CircularIn,
        CircularOut,
        CircularInOut,
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        BackIn,
        BackOut,
        BackInOut,
        BounceIn,
        BounceOut,
        BounceInOut
    }

    public static class EasingExtensions
    {
        public static Func<float, float> GetFunction(this Easing easing)
        {
            return easing switch
            {
                Easing.Linear => Easings.Linear,
                Easing.QuadraticIn => Easings.Quadratic.In,
                Easing.QuadraticOut => Easings.Quadratic.Out,
                Easing.QuadraticInOut => Easings.Quadratic.InOut,
                Easing.CubicIn => Easings.Cubic.In,
                Easing.CubicOut => Easings.Cubic.Out,
                Easing.CubicInOut => Easings.Cubic.InOut,
                Easing.QuarticIn => Easings.Quartic.In,
                Easing.QuarticOut => Easings.Quartic.Out,
                Easing.QuarticInOut => Easings.Quartic.InOut,
                Easing.QuinticIn => Easings.Quintic.In,
                Easing.QuinticOut => Easings.Quintic.Out,
                Easing.QuinticInOut => Easings.Quintic.InOut,
                Easing.SinusoidalIn => Easings.Sinusoidal.In,
                Easing.SinusoidalOut => Easings.Sinusoidal.Out,
                Easing.SinusoidalInOut => Easings.Sinusoidal.InOut,
                Easing.ExponentialIn => Easings.Exponential.In,
                Easing.ExponentialOut => Easings.Exponential.Out,
                Easing.ExponentialInOut => Easings.Exponential.InOut,
                Easing.CircularIn => Easings.Circular.In,
                Easing.CircularOut => Easings.Circular.Out,
                Easing.CircularInOut => Easings.Circular.InOut,
                Easing.ElasticIn => Easings.Elastic.In,
                Easing.ElasticOut => Easings.Elastic.Out,
                Easing.ElasticInOut => Easings.Elastic.InOut,
                Easing.BackIn => Easings.Back.In,
                Easing.BackOut => Easings.Back.Out,
                Easing.BackInOut => Easings.Back.InOut,
                Easing.BounceIn => Easings.Bounce.In,
                Easing.BounceOut => Easings.Bounce.Out,
                Easing.BounceInOut => Easings.Bounce.InOut,
                _ => throw new ArgumentOutOfRangeException(nameof(easing), easing, null)
            };
        }
    }

    // Based on easings.net
    public static class Easings
    {
        public static float Linear(float v)
        {
            return v;
        }

        public static class Quadratic
        {
            public static float In(float v)
            {
                return v * v;
            }

            public static float Out(float v)
            {
                return v * (2f - v);
            }

            public static float InOut(float v)
            {
                return (v *= 2f) < 1f ? 0.5f * In(v) : -0.5f * ((v -= 1f) * (v - 2f) - 1f);
            }
        }

        public static class Cubic
        {
            public static float In(float v)
            {
                return v * v * v;
            }

            public static float Out(float v)
            {
                return 1f + (v -= 1f) * v * v;
            }

            public static float InOut(float v)
            {
                return (v *= 2f) < 1f ? 0.5f * In(v) : 0.5f * ((v -= 2f) * v * v + 2f);
            }
        }

        public static class Quartic
        {
            public static float In(float v)
            {
                return v * v * v * v;
            }

            public static float Out(float v)
            {
                return 1f - (v -= 1f) * v * v * v;
            }

            public static float InOut(float v)
            {
                return (v *= 2f) < 1f ? 0.5f * In(v) : -0.5f * ((v -= 2f) * v * v * v - 2f);
            }
        }

        public static class Quintic
        {
            public static float In(float v)
            {
                return v * v * v * v * v;
            }

            public static float Out(float v)
            {
                return 1f + (v -= 1f) * v * v * v * v;
            }

            public static float InOut(float v)
            {
                return (v *= 2f) < 1f ? 0.5f * In(v) : 0.5f * ((v -= 2f) * v * v * v * v + 2f);
            }
        }

        public static class Sinusoidal
        {
            public static float In(float v)
            {
                return 1f - Mathf.Cos(v * Mathf.PI / 2f);
            }

            public static float Out(float v)
            {
                return Mathf.Sin(v * Mathf.PI / 2f);
            }

            public static float InOut(float v)
            {
                return 0.5f * (1f - Mathf.Cos(Mathf.PI * v));
            }
        }

        public static class Exponential
        {
            public static float In(float v)
            {
                return Mathf.Abs(v) < Mathf.Epsilon ? 0f : Mathf.Pow(1024f, v - 1f);
            }

            public static float Out(float v)
            {
                return Mathf.Abs(v - 1f) < Mathf.Epsilon ? 1f : 1f - Mathf.Pow(2f, -10f * v);
            }

            public static float InOut(float v)
            {
                if (Mathf.Abs(v) < Mathf.Epsilon) return 0f;
                if (Mathf.Abs(v - 1f) < Mathf.Epsilon) return 1f;
                return (v *= 2f) < 1f ? 0.5f * Mathf.Pow(1024f, v - 1f) : 0.5f * (-Mathf.Pow(2f, -10f * (v - 1f)) + 2f);
            }
        }

        public static class Circular
        {
            public static float In(float v)
            {
                return 1f - Mathf.Sqrt(1f - v * v);
            }

            public static float Out(float v)
            {
                return Mathf.Sqrt(1f - (v -= 1f) * v);
            }

            public static float InOut(float v)
            {
                return (v *= 2f) < 1f
                    ? -0.5f * (Mathf.Sqrt(1f - v * v) - 1)
                    : 0.5f * (Mathf.Sqrt(1f - (v -= 2f) * v) + 1f);
            }
        }

        public static class Elastic
        {
            public static float In(float v)
            {
                if (Mathf.Abs(v) < Mathf.Epsilon) return 0;
                if (Mathf.Abs(v - 1) < Mathf.Epsilon) return 1;
                return -Mathf.Pow(2f, 10f * (v -= 1f)) * Mathf.Sin((v - 0.1f) * (2f * Mathf.PI) / 0.4f);
            }

            public static float Out(float v)
            {
                if (Mathf.Abs(v) < Mathf.Epsilon) return 0;
                if (Mathf.Abs(v - 1) < Mathf.Epsilon) return 1;
                return Mathf.Pow(2f, -10f * v) * Mathf.Sin((v - 0.1f) * (2f * Mathf.PI) / 0.4f) + 1f;
            }

            public static float InOut(float v)
            {
                return (v *= 2f) < 1f
                    ? -0.5f * Mathf.Pow(2f, 10f * (v -= 1f)) * Mathf.Sin((v - 0.1f) * (2f * Mathf.PI) / 0.4f)
                    : Mathf.Pow(2f, -10f * (v -= 1f)) * Mathf.Sin((v - 0.1f) * (2f * Mathf.PI) / 0.4f) * 0.5f + 1f;
            }
        }

        public static class Back
        {
            private const float S = 1.70158f;
            private const float S2 = 2.5949095f;

            public static float In(float v)
            {
                return v * v * ((S + 1f) * v - S);
            }

            public static float Out(float v)
            {
                return (v -= 1f) * v * ((S + 1f) * v + S) + 1f;
            }

            public static float InOut(float v)
            {
                return (v *= 2f) < 1f
                    ? 0.5f * (v * v * ((S2 + 1f) * v - S2))
                    : 0.5f * ((v -= 2f) * v * ((S2 + 1f) * v + S2) + 2f);
            }
        }

        public static class Bounce
        {
            public static float In(float v)
            {
                return 1f - Out(1f - v);
            }

            public static float Out(float v)
            {
                return v switch
                {
                    < 1f / 2.75f => 7.5625f * v * v,
                    < 2f / 2.75f => 7.5625f * (v -= 1.5f / 2.75f) * v + 0.75f,
                    < 2.5f / 2.75f => 7.5625f * (v -= 2.25f / 2.75f) * v + 0.9375f,
                    _ => 7.5625f * (v -= 2.625f / 2.75f) * v + 0.984375f
                };
            }

            public static float InOut(float v)
            {
                return v < 0.5f ? In(v * 2f) * 0.5f : Out(v * 2f - 1f) * 0.5f + 0.5f;
            }
        }
    }
}