using UnityEngine;

namespace DeepDreams.Interactions
{
    public class InteractableBase : MonoBehaviour, IInteractable
    {
        public float HoldDuration { get; }
        public float HoldInteract { get; }
        public float MultipleUse { get; }
        public float IsInteractable { get; }

        public InteractableBase(float holdDuration, float holdInteract, float multipleUse, float isInteractable) {
            HoldDuration = holdDuration;
            HoldInteract = holdInteract;
            MultipleUse = multipleUse;
            IsInteractable = isInteractable;
        }

        public void OnStartHover() {
            Debug.Log("Start Hovered: " + gameObject.name);
        }

        public void OnInteract() {
            Debug.Log("Interacted: " + gameObject.name);
        }

        public void OnEndHover() {
            Debug.Log("End Hovered: " + gameObject.name);
        }
    }
}