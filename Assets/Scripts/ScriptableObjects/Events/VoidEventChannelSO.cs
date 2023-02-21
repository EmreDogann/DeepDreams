using System.Collections.Generic;
using UnityEngine;

namespace DeepDreams.ScriptableObjects.Events
{
    [CreateAssetMenu(fileName = "New Void Event", menuName = "Game Event/Void Event", order = 1)]
    public class VoidEventChannelSO : ScriptableObject
    {
        private List<VoidEventListener> listeners = new List<VoidEventListener>();

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