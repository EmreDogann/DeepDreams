using DeepDreams.SaveLoad;
using UnityEditor;
using UnityEngine;

namespace SaveLoad.Editor
{
    [CustomPropertyDrawer(typeof(SaveDataFieldReference<>))]
    public class DataPersistenceFieldReferenceDrawer : PropertyDrawer
    {
        private int _choiceIndex;
        private string[] _fieldNames;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty selectedFieldProperty = property.FindPropertyRelative("selectedField");
            SerializedProperty fieldNamesProperty = property.FindPropertyRelative("fieldNames");
            SerializedProperty selectedNameProperty = property.FindPropertyRelative("selectedName");

            _fieldNames = new string[fieldNamesProperty.arraySize];

            for (int i = 0; i < fieldNamesProperty.arraySize; i++)
            {
                _fieldNames[i] = fieldNamesProperty.GetArrayElementAtIndex(i).stringValue;
            }

            position.y += EditorGUIUtility.singleLineHeight;
            position.height -= EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();
            _choiceIndex = EditorGUI.Popup(position, label.text, selectedFieldProperty.intValue, _fieldNames);

            if (EditorGUI.EndChangeCheck())
            {
                selectedFieldProperty.intValue = _choiceIndex;
                selectedNameProperty.stringValue = _fieldNames[_choiceIndex].Split(" (")[0];
            }

            EditorGUI.EndProperty();
        }
    }
}