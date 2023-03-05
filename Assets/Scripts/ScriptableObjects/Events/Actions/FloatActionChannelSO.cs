using System.Collections.Generic;
using UnityEngine;

namespace DeepDreams.ScriptableObjects.Events.Actions
{
    [CreateAssetMenu(fileName = "New Float Event", menuName = "Game Event/Actions/Float Event", order = 1)]
    public class FloatActionChannelSO : ScriptableObject
    {
        private readonly List<FloatActionListener> listeners = new List<FloatActionListener>();

        public void Raise(float value)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(value);
            }
        }

        public void RegisterListener(FloatActionListener listener)
        {
            listeners.Add(listener);
        }

        public void UnregisterListener(FloatActionListener listener)
        {
            listeners.Remove(listener);
        }
    }
}