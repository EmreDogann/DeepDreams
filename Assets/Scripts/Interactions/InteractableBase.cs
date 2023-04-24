using System;
using UnityEngine;

namespace DeepDreams.Interactions
{
    public class InteractionData
    {
        public Transform Source;
        public Vector3 InteractionPoint;
        public Vector3 InteractionForce;

        public InteractionData(Transform sourceTransform, Vector3 interactionPoint, Vector3 interactionForce)
        {
            Source = sourceTransform;
            InteractionPoint = interactionPoint;
            InteractionForce = interactionForce;
        }

        public InteractionData()
        {
            Source = null;
            InteractionPoint = Vector3.zero;
            InteractionForce = Vector3.zero;
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

        public virtual void OnStartInteract(InteractionData interactionData)
        {
            Debug.Log("Interacted: " + gameObject.name);
        }

        public virtual void OnInteract(InteractionData interactionData)
        {
            Debug.Log("Interacted: " + gameObject.name);
        }

        public virtual void OnEndInteract(InteractionData interactionData)
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