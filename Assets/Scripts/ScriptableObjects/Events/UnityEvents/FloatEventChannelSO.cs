using System.Collections.Generic;
using UnityEngine;

namespace DeepDreams.ScriptableObjects.Events.UnityEvents
{
    [CreateAssetMenu(fileName = "New Float Event", menuName = "Game Event/Unity Event/Float Event", order = 2)]
    public class FloatEventChannelSO : ScriptableObject
    {
        private readonly List<FloatEventListener> listeners = new List<FloatEventListener>();

        public void Raise(bool value)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(value);
            }
        }

        public void RegisterListener(FloatEventListener listener)
        {
            listeners.Add(listener);
        }

        public void UnregisterListener(FloatEventListener listener)
        {
            listeners.Remove(listener);
        }
    }
}