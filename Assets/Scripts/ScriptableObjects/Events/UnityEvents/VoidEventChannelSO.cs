using System.Collections.Generic;
using UnityEngine;

namespace DeepDreams.ScriptableObjects.Events.UnityEvents
{
    [CreateAssetMenu(fileName = "New Void Event", menuName = "Game Event/Unity Event/Void Event", order = 3)]
    public class VoidEventChannelSO : ScriptableObject
    {
        private readonly List<VoidEventListener> listeners = new List<VoidEventListener>();

        public void Raise()
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised();
            }
        }

        public void RegisterListener(VoidEventListener listener)
        {
            listeners.Add(listener);
        }

        public void UnregisterListener(VoidEventListener listener)
        {
            listeners.Remove(listener);
        }
    }
}