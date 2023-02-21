// file Touchable.cs
// Correctly backfills the missing Touchable concept in Unity.UI's OO chain.
// Original From: https://forum.unity.com/threads/invisible-buttons.267245/
// Modified from: https://stackoverflow.com/a/64074858/10439539


using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DeepDreams.UI
{
#if UNITY_EDITOR
    // Removes all the inspector UI for this component so it appears blank.
    [CustomEditor(typeof(Touchable))]
    public class TouchableEditor : Editor
    {
        public override void OnInspectorGUI() {}
    }
#endif
    [RequireComponent(typeof(CanvasRenderer))]
    public class Touchable : Graphic
    {
        protected override void UpdateGeometry() {}
    }
}