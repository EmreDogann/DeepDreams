using System;
using UnityEngine;

namespace DeepDreams.ScriptableObjects.Audio
{
    [Serializable]
    [CreateAssetMenu(menuName = "Audio/Bus Reference", fileName = "New Bus Reference")]
    public class BusReference : ScriptableObject
    {
        public string busPath;
    }
}