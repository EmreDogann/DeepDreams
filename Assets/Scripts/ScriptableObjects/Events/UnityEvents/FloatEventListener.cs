using System;
using UnityEngine;
using UnityEngine.Events;

namespace DeepDreams.ScriptableObjects.Events.UnityEvents
{
    /// <summary>
    ///     To use a generic UnityEvent type you must override the generic type.
    /// </summary>
    [Serializable]
    public class FloatEvent : UnityEvent<bool> {}

    public class FloatEventListener : MonoBehaviour
    {
        public FloatEventChannelSO Event;
        public FloatEvent Response;

        private void OnEnable()
        {
            Event.RegisterListener(this);
        }

        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }

        public void OnEventRaised(bool value)
        {
            Response.Invoke(value);
        }
    }
}