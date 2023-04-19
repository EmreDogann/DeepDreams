using System;
using UnityEngine;

namespace DeepDreams.Interactions
{
    public class InteractionSource
    {
        public Vector3 position;
        public Vector3 direction;

        public InteractionSource(Vector3 position, Vector3 direction)
        {
            this.position = position;
            this.direction = direction;
        }
    }

    [Serializable]
    public class InteractableBase : MonoBehaviour, IInteractable
    {
        [field: SerializeField]
        public float HoldDuration { get; protected set; }
        [field: SerializeField]
        public bool HoldInteract { get; protected set; }
        [field: SerializeField]
        public float MultipleUse { get; protected set; }
        [field: SerializeField]
        public bool IsInteractable { get; protected set; }

        protected float HoldProgress = 0.0f;

        public InteractableBase(float holdDuration, bool holdInteract, float multipleUse, bool isInteractable)
        {
            HoldDuration = holdDuration;
            HoldInteract = holdInteract;
            MultipleUse = multipleUse;
            IsInteractable = isInteractable;
        }

        public virtual void OnStartHover()
        {
            Debug.Log("Start Hovered: " + gameObject.name);
        }

        public virtual void OnInteract()
        {
            Debug.Log("Interacted: " + gameObject.name);
        }

        public virtual void OnInteract(InteractionSource interactionSource = null)
        {
            Debug.Log("Interacted: " + gameObject.name);
        }

        public virtual void OnEndInteract()
        {
            Debug.Log("End Interacted: " + gameObject.name);
        }

        public virtual void OnEndHover()
        {
            Debug.Log("End Hovered: " + gameObject.name);
        }

        public bool IsHoldInteractFinished()
        {
            return HoldProgress >= HoldDuration;
        }
    }
}