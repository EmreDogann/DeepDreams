using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DeepDreams.UI.Components.Buttons
{
    public class ExitButton : MenuButton
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}