using DeepDreams.Utils.Attributes;
using UnityEditor;
using UnityEngine;

namespace DeepDreams.Utils.Editor
{
    [CustomPropertyDrawer(typeof(OrthogonalUnitVector3Attribute))] [CanEditMultipleObjects]
    public class OrthogonalUnitVector3Drawer : PropertyDrawer
    {
        private readonly string[] _enumNames = { "X", "Y", "Z" };
        private int _axis;
        private bool _invert;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get existing axis value.
            if (property.vector3Value.x != 0)
            {
                _axis = 0;
                _invert = !(property.vector3Value.x > 0);
            }
            else if (property.vector3Value.y != 0)
            {
                _axis = 1;
                _invert = !(property.vector3Value.y > 0);
            }
            else
            {
                _axis = 2;
                _invert = !(property.vector3Value.z > 0);
            }

            Rect initialRect = position;

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Wrap property drawer fields to new line if screen size is too small.
            if (Screen.width < 332)
            {
                position.x = initialRect.xMin;
                position.y += EditorGUIUtility.singleLineHeight;
                position.width = initialRect.width;
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.indentLevel = 1;
                position = EditorGUI.IndentedRect(position);
            }

            EditorGUI.indentLevel = 0;

            Rect column = new Rect(position.x, position.y, position.width / 3, position.height);
            Rect axisRect = new Rect(column.x, column.y, column.width / 4, column.height);
            Rect invertRect = new Rect(column.xMax, column.y, column.width / 4 * 3, column.height);
            Rect vector3Rect = new Rect(column.xMax + 2, column.y, position.width / 3 * 2 - 2, position.height);

            EditorGUI.BeginChangeCheck();
            int chosenAxis = EditorGUI.Popup(axisRect, _axis, _enumNames, EditorStyles.miniButton);
            GUIContent toggleLabel = new GUIContent("Invert");
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(toggleLabel).x + 2;

            invertRect.x -= EditorStyles.toggle.CalcSize(toggleLabel).x + EditorStyles.toggle.margin.right +
                            EditorStyles.toggle.margin.left;
            bool chosenInvert = EditorGUI.Toggle(invertRect, toggleLabel, _invert, EditorStyles.toggle);
            EditorGUIUtility.labelWidth = labelWidth;

            if (EditorGUI.EndChangeCheck())
            {
                _axis = chosenAxis;
                _invert = chosenInvert;
                if (chosenInvert) SetVectorComponent(property, _enumNames[_axis], -1.0f);
                else SetVectorComponent(property, _enumNames[_axis], 1.0f);
            }

            GUI.enabled = false;
            EditorGUI.Vector3Field(vector3Rect, "", property.vector3Value);
            GUI.enabled = true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Screen.width < 332 ? EditorGUIUtility.singleLineHeight * 2 : EditorGUIUtility.singleLineHeight;
        }

        private void SetVectorComponent(SerializedProperty property, string componentName, float invert)
        {
            if (componentName == "X") property.vector3Value = Vector3.right * invert;
            else if (componentName == "Y") property.vector3Value = Vector3.up * invert;
            else property.vector3Value = Vector3.forward * invert;
        }
    }
}