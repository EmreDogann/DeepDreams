using System;
using UnityEngine;

namespace DeepDreams.UI.Views.Transitions
{
    [Serializable]
    public class MenuTransitionFactory
    {
        public enum MenuTransitionType
        {
            Simple,
            Fade
        }
        public MenuTransitionType TransitionType = MenuTransitionType.Simple;

        [SerializeField] private SimpleMenuTransition SimpleMenuTransition = new SimpleMenuTransition();
        [SerializeField] private FadeMenuTransition FadeMenuTransition = new FadeMenuTransition();

        public MenuTransition CreateTransition()
        {
            return GetTransitionFromType(TransitionType);
        }

        public Type GetClassType(MenuTransitionType menuType)
        {
            return GetTransitionFromType(menuType).GetType();
        }

        private MenuTransition GetCurrentTransition()
        {
            return GetTransitionFromType(TransitionType);
        }

        private MenuTransition GetTransitionFromType(MenuTransitionType type)
        {
            switch (type)
            {
                case MenuTransitionType.Simple:
                    return SimpleMenuTransition;
                case MenuTransitionType.Fade:
                    return FadeMenuTransition;
                default:
                    return SimpleMenuTransition;
            }
        }
    }
}