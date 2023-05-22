using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DeepDreams.RenderFeatures.Editor
{
#if UNITY_EDITOR
    [CustomEditor(typeof(CustomBlurRenderFeature))]
    public class CustomBlurRenderFeatureEditor : UnityEditor.Editor
    {
        private SerializedProperty renderPassEvent;

        private SerializedProperty showInSceneView;
        private SerializedProperty hdrFiltering;
        private SerializedProperty dithering;
        private SerializedProperty blurModes;
        private SerializedProperty blurTextureName;
        private SerializedProperty copyToCameraFramebuffer;

        private SerializedProperty gaussianSettings;
        private SerializedProperty kawaseDualFilterSettings;

        private void OnEnable()
        {
            renderPassEvent = serializedObject.FindProperty(nameof(CustomBlurRenderFeature.renderPassEvent));

            SerializedProperty featureSettings =
                serializedObject.FindProperty(nameof(CustomBlurRenderFeature.settings));
            showInSceneView = featureSettings.FindPropertyRelative(nameof(showInSceneView));
            hdrFiltering = featureSettings.FindPropertyRelative(nameof(hdrFiltering));
            dithering = featureSettings.FindPropertyRelative(nameof(dithering));
            blurModes = featureSettings.FindPropertyRelative(nameof(blurModes));
            blurTextureName =
                featureSettings.FindPropertyRelative(nameof(blurTextureName));
            copyToCameraFramebuffer =
                featureSettings.FindPropertyRelative(nameof(copyToCameraFramebuffer));

            gaussianSettings = serializedObject.FindProperty(nameof(CustomBlurRenderFeature.gaussianSettings));
            kawaseDualFilterSettings =
                serializedObject.FindProperty(nameof(CustomBlurRenderFeature.kawaseDualFilterSettings));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(renderPassEvent);
            EditorGUILayout.PropertyField(copyToCameraFramebuffer);

            if (copyToCameraFramebuffer.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(showInSceneView);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(hdrFiltering, new GUIContent("HDR Filtering"));
            EditorGUILayout.PropertyField(dithering);

            blurModes.intValue =
                (int)(CustomBlurRenderFeature.BlurModes)EditorGUILayout.EnumPopup("Blur Mode",
                    (CustomBlurRenderFeature.BlurModes)blurModes.intValue);

            switch ((CustomBlurRenderFeature.BlurModes)blurModes.intValue)
            {
                case CustomBlurRenderFeature.BlurModes.Gaussian:
                    // EditorGUILayout.PropertyField(gaussianSettings, true);
                    DrawClassProperties(gaussianSettings);
                    break;
                case CustomBlurRenderFeature.BlurModes.KawaseDualFilter:
                    // EditorGUILayout.PropertyField(kawaseDualFilterSettings);
                    DrawClassProperties(kawaseDualFilterSettings);
                    break;
            }

            EditorGUILayout.PropertyField(blurTextureName);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawClassProperties(SerializedProperty property)
        {
            foreach (SerializedProperty directChild in GetVisibleChildren(property))
            {
                EditorGUILayout.PropertyField(directChild, true);
            }
        }

        // https://forum.unity.com/threads/loop-through-serializedproperty-children.435119/
        /// <summary>
        ///     Gets visible children of `SerializedProperty` at 1 level depth.
        /// </summary>
        /// <param name="property">Parent `SerializedProperty`.</param>
        /// <returns>Collection of `SerializedProperty` children.</returns>
        private IEnumerable<SerializedProperty> GetVisibleChildren(SerializedProperty property)
        {
            SerializedProperty currentProperty = property.Copy();
            SerializedProperty nextSiblingProperty = property.Copy();
            {
                nextSiblingProperty.NextVisible(false);
            }

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                    {
                        break;
                    }

                    yield return currentProperty;
                } while (currentProperty.NextVisible(false));
            }
        }
    }
#endif
}