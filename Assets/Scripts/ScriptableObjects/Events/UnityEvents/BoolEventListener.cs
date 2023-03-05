using System;
using UnityEngine;
using UnityEngine.Events;

namespace DeepDreams.ScriptableObjects.Events.UnityEvents
{
    /// <summary>
    ///     To use a generic UnityEvent type you must override the generic type.
    /// </summary>
    [Serializable]
    public class BoolEvent : UnityEvent<bool> {}

    public class BoolEventListener : MonoBehaviour
    {
        public BoolEventChannelSO Event;
        public BoolEvent Response;

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