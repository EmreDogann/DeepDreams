using System.Collections.Generic;
using UnityEngine;

namespace DeepDreams.ScriptableObjects.Events.UnityEvents
{
    [CreateAssetMenu(fileName = "New Bool Event", menuName = "Game Event/Unity Event/Bool Event", order = 2)]
    public class BoolEventChannelSO : ScriptableObject
    {
        private readonly List<BoolEventListener> listeners = new List<BoolEventListener>();

        public void Raise(bool value)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(value);
            }
        }

        public void RegisterListener(BoolEventListener listener)
        {
            listeners.Add(listener);
        }

        public void UnregisterListener(BoolEventListener listener)
        {
            listeners.Remove(listener);
        }
    }
}