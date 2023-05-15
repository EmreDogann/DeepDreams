using System;
using UnityEngine;

namespace DeepDreams.Utils
{
    public class CallAfterDelay : MonoBehaviour
    {
        private float _delay;
        private Action _action;

        // Will never call this frame, always the next frame at the earliest
        public static CallAfterDelay Create(float delay, Action action)
        {
            CallAfterDelay cad = new GameObject("CallAfterDelay").AddComponent<CallAfterDelay>();
            cad._delay = delay;
            cad._action = action;
            return cad;
        }

        private float age;

        private void Update()
        {
            if (age > _delay)
            {
                _action();
                Destroy(gameObject);
            }
        }

        private void LateUpdate()
        {
            age += Time.deltaTime;
        }
    }
}