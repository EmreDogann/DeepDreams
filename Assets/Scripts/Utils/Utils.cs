using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DeepDreams.Utils
{
    public static class UtilHelpers
    {
        public static void ResetCurve(AnimationCurve curve)
        {
            for (int i = curve.keys.Length - 1; i >= 0; i--)
            {
                curve.RemoveKey(i);
            }
        }

        public static void SetSineCurve(AnimationCurve curve)
        {
            ResetCurve(curve);

            float maxValue = 2 * Mathf.PI;
            curve.postWrapMode = WrapMode.Loop;
            curve.AddKey(new Keyframe(0, 0, maxValue, maxValue)); // 0, sin(0), sin'(0) = cos(0), sin'(0) = cos(0)
            curve.AddKey(new Keyframe(0.5f * Mathf.PI / maxValue, 1, 0, 0));
            curve.AddKey(new Keyframe(Mathf.PI / maxValue, 0, -maxValue, -maxValue));
            curve.AddKey(new Keyframe(1.5f * Mathf.PI / maxValue, -1, 0, 0));
            curve.AddKey(new Keyframe(2f * Mathf.PI / maxValue, 0, maxValue, maxValue));
        }

        public static void SetCosineCurve(AnimationCurve curve)
        {
            ResetCurve(curve);

            float maxValue = 2 * Mathf.PI;
            curve.postWrapMode = WrapMode.Loop;
            curve.AddKey(new Keyframe(0, 1, 0, 0));
            curve.AddKey(new Keyframe(0.5f * Mathf.PI / maxValue, 0, -maxValue, -maxValue));
            curve.AddKey(new Keyframe(Mathf.PI / maxValue, -1, 0, 0));
            curve.AddKey(new Keyframe(1.5f * Mathf.PI / maxValue, 0, maxValue, maxValue));
            curve.AddKey(new Keyframe(2f * Mathf.PI / maxValue, 1, 0, 0));
        }

        // From: https://stackoverflow.com/questions/30817924/obtain-non-explicit-field-offset/56512720#56512720
        public static int GetFieldOffset(this FieldInfo fi)
        {
            return GetFieldOffset(fi.FieldHandle);
        }

        // From: https://stackoverflow.com/questions/30817924/obtain-non-explicit-field-offset/56512720#56512720
        public static int GetFieldOffset(RuntimeFieldHandle h)
        {
            return Marshal.ReadInt32(h.Value + (4 + IntPtr.Size)) & 0xFFFFFF;
        }

        // From: https://stackoverflow.com/questions/30817924/obtain-non-explicit-field-offset/56512720#56512720
        /// <summary>
        ///     Returns a managed reference ("interior pointer") to the value or instance of type 'U'
        ///     stored in the field indicated by 'fi' within managed object instance 'obj'
        /// </summary>
        public static unsafe ref U RefFieldValue<U>(object obj, FieldInfo fi)
        {
            IntPtr pobj = Unsafe.As<object, IntPtr>(ref obj);
            pobj += IntPtr.Size + GetFieldOffset(fi.FieldHandle);
            return ref Unsafe.AsRef<U>(pobj.ToPointer());
        }
    }

    // From: https://stackoverflow.com/questions/38528620/c-sharp-fieldinfo-reflection-alternatives
    public class FieldAccessor
    {
        private static readonly ParameterExpression fieldParameter = Expression.Parameter(typeof(object));
        private static readonly ParameterExpression ownerParameter = Expression.Parameter(typeof(object));

        public FieldAccessor(Type type, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) throw new ArgumentException();

            MemberExpression fieldExpression = Expression.Field(
                Expression.Convert(ownerParameter, type), field);

            Get = Expression.Lambda<Func<object, object>>(
                Expression.Convert(fieldExpression, typeof(object)),
                ownerParameter).Compile();

            Set = Expression.Lambda<Action<object, object>>(
                Expression.Assign(fieldExpression,
                    Expression.Convert(fieldParameter, field.FieldType)),
                ownerParameter, fieldParameter).Compile();
        }

        public Func<object, object> Get { get; }

        public Action<object, object> Set { get; }
    }
}