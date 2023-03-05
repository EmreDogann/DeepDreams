using System;
using UnityEngine;
using UnityEngine.Events;

namespace DeepDreams.ScriptableObjects.Events.UnityEvents
{
    /// <summary>
    ///     To use a generic UnityEvent type you must override the generic type.
    /// </summary>
    [Serializable]
    public class RecoilEvent : UnityEvent<Vector3, float, float> {}

    public class RecoilEventListener : MonoBehaviour
    {
        public RecoilEventChannelSO Event;
        public RecoilEvent Response;

        private void OnEnable()
        {
            Event.RegisterListener(this);
        }

        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }

        public void OnEventRaised(Vector3 valueA, float valueB, float valueC)
        {
            Response.Invoke(valueA, valueB, valueC);
        }
    }
}