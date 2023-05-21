using System;
using UnityEditor;

namespace DeepDreams.Utils
{
    // From: https://gist.github.com/FreyaHolmer/60d76ed842a6c4f8f1ed424d6866575b
    public static class IMGUIDebugger
    {
        private static readonly Type type = Type.GetType("UnityEditor.GUIViewDebuggerWindow,UnityEditor");

        [MenuItem("Window/IMGUI Debugger")]
        public static void Open()
        {
            EditorWindow.GetWindow(type).Show();
        }
    }
}