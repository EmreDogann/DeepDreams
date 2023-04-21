using DeepDreams.Utils.Attributes;
using MyBox;
using UnityEngine;

namespace DeepDreams.Interactions
{
    public class Drawer : InteractableBase
    {
        [Separator("General")]
        public float min = -90.0f;
        public float max = 90.0f;
        public float mass = 1.0f;
        public float drag = 0.1f;
        [OrthogonalUnitVector3] public Vector3 axis = Vector3.forward;

        [ReadOnly] [SerializeField] private Vector3 velocity;
        private bool _isInteracting;

        public Drawer(float holdDuration, bool holdInteract, float multipleUse, bool isInteractable) : base(holdDuration, holdInteract,
            multipleUse, isInteractable) {}

        private void Update()
        {
            Vector3 newPosition = Vector3.zero;

            velocity *= Mathf.Clamp01(1.0f - drag * Time.deltaTime);
            newPosition += velocity * Time.deltaTime;
            newPosition += transform.localPosition;
            ComponentClamp(ref newPosition, min, max);
            transform.localPosition = newPosition;

            if (ReachedLimit()) {}
        }

        public override void OnInteract(InteractionData interactionData)
        {
            _isInteracting = true;
            // base.OnInteract();

            // Debug.DrawRay(transform.position, Vector3.forward, Color.blue);
            // Debug.DrawRay(transform.position, Vector3.right, Color.red);

            float dotProductRight = Vector3.Dot(Vector3.right, interactionData.SourceDirection);
            float dotProductForward = Vector3.Dot(Vector3.forward, interactionData.SourceDirection);

            velocity += axis * (interactionData.InteractionForce.x / mass * dotProductForward +
                                interactionData.InteractionForce.y / mass * dotProductRight);
        }

        public override void OnEndInteract()
        {
            _isInteracting = false;
        }

        private bool ReachedLimit()
        {
            float component = Vector3.Dot(Vector3.Scale(transform.localPosition, axis.normalized), axis.normalized);
            if (component == min || component == max) return true;

            return false;
        }

        private void ComponentClamp(ref Vector3 vec, float min, float max)
        {
            vec.x = Mathf.Clamp(vec.x, min, max);
            vec.y = Mathf.Clamp(vec.y, min, max);
            vec.z = Mathf.Clamp(vec.z, min, max);
        }
    }
}