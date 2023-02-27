using System;
using DeepDreams.UI.Views;
using MyBox;
using TypeReferences;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DeepDreams.UI.Components.Buttons
{
    public class SwitchViewButton : MenuButton
    {
        [Separator("Switch View")]
        [SerializeField] private SwitcherMode switchMode;

        [ReadOnly(nameof(switchMode), false, SwitcherMode.Back, SwitcherMode.None)]
        [Inherits(typeof(View), ShowNoneElement = false, Grouping = Grouping.None, ShortName = true)]
        [SerializeField] private TypeReference targetView;

        private Action<UIManager, bool> _showMethodAction;

        private void Start()
        {
            if (switchMode != SwitcherMode.Back && switchMode != SwitcherMode.None)
                _showMethodAction = UIManager.instance.GetCachedMethod(targetView);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (switchMode == SwitcherMode.Back || switchMode == SwitcherMode.None) targetView = null;
        }
#endif

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            switch (switchMode)
            {
                case SwitcherMode.None:
                    break;
                case SwitcherMode.Back:
                    UIManager.instance.Back();
                    break;
                case SwitcherMode.Replace:
                    _showMethodAction(UIManager.instance, false);
                    break;
                case SwitcherMode.Add:
                    _showMethodAction(UIManager.instance, true);
                    break;
            }
        }

        private enum SwitcherMode
        {
            None,
            Back,
            Replace,
            Add
        }
    }
}