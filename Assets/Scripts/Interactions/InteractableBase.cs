using System;
using UnityEngine;

namespace DeepDreams.Interactions
{
    public class InteractionData
    {
        public Vector3 SourcePosition;
        public Vector3 SourceDirection;
        public Vector3 InteractionPoint;
        public Vector3 InteractionForce;

        public InteractionData(Vector3 position, Vector3 direction, Vector3 interactionPoint, Vector3 interactionForce)
        {
            SourcePosition = position;
            SourceDirection = direction;
            InteractionPoint = interactionPoint;
            InteractionForce = interactionForce;
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
            // Debug.Log("Start Hovered: " + gameObject.name);
        }

        public virtual void OnStartInteract()
        {
            // Debug.Log("End Interacted: " + gameObject.name);
        }

        public virtual void OnInteract()
        {
            Debug.Log("Interacted: " + gameObject.name);
        }

        public virtual void OnInteract(InteractionData interactionData)
        {
            Debug.Log("Interacted: " + gameObject.name);
        }

        public virtual void OnEndInteract()
        {
            // Debug.Log("End Interacted: " + gameObject.name);
        }

        public virtual void OnEndHover()
        {
            // Debug.Log("End Hovered: " + gameObject.name);
        }

        public bool IsHoldInteractFinished()
        {
            return HoldProgress >= HoldDuration;
        }
    }
}