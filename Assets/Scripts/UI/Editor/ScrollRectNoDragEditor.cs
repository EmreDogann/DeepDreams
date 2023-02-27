using DeepDreams.UI.Components;
using UnityEditor;
using UnityEditor.UI;

namespace DeepDreams.UI.Editor
{
    [CustomEditor(typeof(ScrollRectNoDrag), true)]
    [CanEditMultipleObjects]
    public class ScrollRectNoDragEditor : ScrollRectEditor
    {
        private SerializedProperty optionSpacing;
        private SerializedProperty numberOfOptions;
        private SerializedProperty optionHeight;
        private SerializedProperty scrollSensitivityStepAmount;

        protected override void OnEnable()
        {
            base.OnEnable();
            optionSpacing = serializedObject.FindProperty("optionSpacing");
            numberOfOptions = serializedObject.FindProperty("numberOfOptions");
            optionHeight = serializedObject.FindProperty("optionHeight");
            scrollSensitivityStepAmount = serializedObject.FindProperty("scrollSensitivityStepAmount");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(optionSpacing);
            EditorGUILayout.PropertyField(numberOfOptions);
            EditorGUILayout.PropertyField(optionHeight);
            EditorGUILayout.PropertyField(scrollSensitivityStepAmount);

            serializedObject.ApplyModifiedProperties();
        }
    }
}