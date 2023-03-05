using System;
using UnityEngine;

namespace DeepDreams.ScriptableObjects.Events.Actions
{
    public class FloatActionListener : MonoBehaviour
    {
        public FloatActionChannelSO Event;
        public Action<float> Response;

        private void OnEnable()
        {
            Event.RegisterListener(this);
        }

        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }

        public void OnEventRaised(float value)
        {
            Response.Invoke(value);
        }
    }
}