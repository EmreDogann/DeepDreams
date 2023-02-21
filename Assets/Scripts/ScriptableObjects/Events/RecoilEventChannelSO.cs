using System.Collections.Generic;
using UnityEngine;

namespace DeepDreams.ScriptableObjects.Events
{
    [CreateAssetMenu(fileName = "New Recoil Event", menuName = "Game Event/Recoil Event", order = 3)]
    public class RecoilEventChannelSO : ScriptableObject
    {
        private List<RecoilEventListener> listeners = new List<RecoilEventListener>();

        public void Raise(Vector3 valueA, float valueB, float valueC)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(valueA, valueB, valueC);
            }
        }

        public void RegisterListener(RecoilEventListener listener)
        {
            listeners.Add(listener);
        }

        public void UnregisterListener(RecoilEventListener listener)
        {
            listeners.Remove(listener);

        }
    }
}