using DeepDreams.SaveLoad;
using DeepDreams.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SaveLoad.Editor
{
    [CustomPropertyDrawer(typeof(SaveFileDescriptor))]
    public class SaveFileDescriptorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty fileNameProperty = property.FindPropertyRelative(nameof(SaveFileDescriptor.fileName));
            SerializedProperty dataTypeProperty = property.FindPropertyRelative(nameof(SaveFileDescriptor.dataType));
            SerializedProperty useEncryptionProperty =
                property.FindPropertyRelative(nameof(SaveFileDescriptor.useEncryption));

            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            // EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // The height of each property must be reset back to the default height otherwise there will be
            // overlapping of click events.
            position.height = base.GetPropertyHeight(property, label);

            int topIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            // To bring back the expand icon.
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                int indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 1;
                position = EditorGUI.IndentedRect(position); // Will indent the current position.
                EditorGUI.indentLevel = indent;

                position.y += base.GetPropertyHeight(property, label) + 2;

                // Draw fields - pass GUIContent.none to each so they are drawn without labels
                EditorGUI.PropertyField(position, fileNameProperty, new GUIContent(fileNameProperty.displayName), true);
                position.y += base.GetPropertyHeight(property, label) + 2;

                EditorGUI.PropertyField(position, dataTypeProperty, new GUIContent(dataTypeProperty.displayName), true);
                position.y += base.GetPropertyHeight(property, label) + 2;

                EditorGUI.PropertyField(position, useEncryptionProperty,
                    new GUIContent(useEncryptionProperty.displayName),
                    true);

                position.width = EditorGUIUtility.currentViewWidth - 15 < 150
                    ? EditorGUIUtility.currentViewWidth - 15
                    : 150;
                position.x =
                    EditorGUIUtility.currentViewWidth / 2 - position.width / 2 + 7; // 7 = Accounts for foldout indents.
                position.height += 2;
                position.y += base.GetPropertyHeight(property, label) + 5;

                EditorStyles.miniButton.alignment = TextAnchor.MiddleCenter;
                EditorGUI.LabelField(position, GUIContent.none);

                if (GUI.Button(new Rect(position), "Clear File Data"))
                {
                    SaveFileDescriptor classInstance = property.GetValue<SaveFileDescriptor>();
                    classInstance.ClearFileData();
                }
            }

            EditorGUI.indentLevel = topIndent;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 'true' is needed to account for the child property heights when this property is part of an array/list.

            // Extra height for clear button.
            float extraHeight = base.GetPropertyHeight(property, label) + 7;
            return EditorGUI.GetPropertyHeight(property, label, true) + (property.isExpanded ? extraHeight : 0);
        }
    }
}