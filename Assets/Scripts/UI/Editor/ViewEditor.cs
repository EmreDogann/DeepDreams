using System;
using System.Reflection;
using DeepDreams.UI.Views;
using DeepDreams.UI.Views.Transitions;
using UnityEditor;
using UnityEngine;

namespace DeepDreams.UI.Editor
{
    [CustomEditor(typeof(View), true)] [CanEditMultipleObjects]
    public class ViewEditor : UnityEditor.Editor
    {
        protected SerializedProperty _serializedFactory;
        private FieldInfo factoryFieldInfo;

        private void OnEnable()
        {
            // Get name of factory member so we don't have to hard-code in the value.
            var fieldInfos = typeof(View).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            for (int i = 0; i < fieldInfos.Length; i++)
            {
                if (fieldInfos[i].FieldType == typeof(MenuTransitionFactory))
                {
                    factoryFieldInfo = fieldInfos[i];
                    break;
                }
            }

            _serializedFactory = serializedObject.FindProperty(factoryFieldInfo.Name);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), GetType(), false);
            }

            EditorGUILayout.Space(16);

            DrawPropertiesExcluding(serializedObject, "m_Script", factoryFieldInfo.Name);
            DrawMenuTransition();
            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawMenuTransition()
        {
            EditorGUILayout.LabelField("Menu Transition", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serializedFactory.FindPropertyRelative(nameof(MenuTransitionFactory.TransitionType)));
            MenuTransitionFactory factoryObject = (MenuTransitionFactory)factoryFieldInfo.GetValue(serializedObject.targetObject);

            if (factoryObject != null)
            {
                Type typeOfTransition = factoryObject.GetClassType(factoryObject.TransitionType);
                SerializedProperty specificTransition = _serializedFactory.FindPropertyRelative(typeOfTransition.Name).Copy();
                string parentPath = specificTransition.propertyPath;

                while (specificTransition.NextVisible(true) && specificTransition.propertyPath.StartsWith(parentPath))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(specificTransition);
                    EditorGUI.indentLevel--;
                }
            }
        }
    }
}