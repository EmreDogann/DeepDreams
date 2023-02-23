using FMODUnity;
using MyBox;
using UnityEngine;

namespace DeepDreams.Audio
{
    public class FMODEvents : MonoBehaviour
    {
        [field: Separator("Footstep SFX")]
        [field: SerializeField] public EventReference walkingFootstep { get; private set; }
        [field: SerializeField] public EventReference runningFootstep { get; private set; }

        [field: Separator("UI SFX")]
        [field: SerializeField] public EventReference UI_Click { get; private set; }
        [field: SerializeField] public EventReference UI_Back { get; private set; }
        [field: SerializeField] public EventReference UI_Hover { get; private set; }
        [field: SerializeField] public EventReference UI_Pause { get; private set; }
        [field: SerializeField] public EventReference UI_StartGame { get; private set; }

        public static FMODEvents instance { get; private set; }

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("Found more than one FMODEvents instance in the scene.");
            }

            instance = this;
        }
    }
}