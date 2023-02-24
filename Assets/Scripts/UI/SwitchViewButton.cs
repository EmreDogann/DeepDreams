using System.Reflection;
using MyBox;
using TypeReferences;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DeepDreams.UI
{
    public class SwitchViewButton : MenuButton
    {
        [Separator("Switch View")]
        [SerializeField] private SwitcherMode switchMode;

        [Inherits(typeof(View), ShowNoneElement = false, Grouping = Grouping.None, ShortName = true)]
        [SerializeField] private TypeReference targetView;
        private readonly object[] showMethodParameters = new object[1];

        private MethodInfo showMethodInfo;

        private void Start()
        {
            if (switchMode != SwitcherMode.Back && switchMode != SwitcherMode.None)
            {
                showMethodInfo = typeof(UIManager).GetMethod(nameof(UIManager.Show), 1, new[] { typeof(bool) })
                    ?.MakeGenericMethod(targetView);
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (switchMode == SwitcherMode.Back || switchMode == SwitcherMode.None)
            {
                targetView = null;
            }
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
                    showMethodParameters[0] = false;
                    showMethodInfo.Invoke(UIManager.instance, showMethodParameters);
                    break;
                case SwitcherMode.Add:
                    showMethodParameters[0] = true;
                    showMethodInfo.Invoke(UIManager.instance, showMethodParameters);
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