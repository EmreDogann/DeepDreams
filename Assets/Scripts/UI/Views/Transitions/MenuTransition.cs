using System;
using System.Collections;

namespace DeepDreams.UI.Views.Transitions
{
    [Serializable]
    public abstract class MenuTransition
    {
        public abstract void Initialize(View view);
        public abstract IEnumerator Hide(View view);
        public abstract IEnumerator Show(View view);
    }

    [Serializable]
    public class SimpleMenuTransition : MenuTransition
    {
        public override void Initialize(View view)
        {
            view.gameObject.SetActive(false);
        }

        public override IEnumerator Show(View view)
        {
            yield break;
        }

        public override IEnumerator Hide(View view)
        {
            yield break;
        }
    }
}