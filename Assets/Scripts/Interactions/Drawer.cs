using DeepDreams.Utils.Attributes;
using MyBox;
using UnityEngine;

namespace DeepDreams.Interactions
{
    public class Drawer : InteractableBase
    {
        [Separator("General")]
        public float min;
        public float max = 1.0f;
        public float mass = 1.0f;
        public float drag = 0.1f;
        public float pushStrength = 100.0f;

        [Tooltip("This is the axis (in local space) the object will move in/rotate around.")]
        [OrthogonalUnitVector3] public Vector3 movementAxis = Vector3.forward;
        [Tooltip(
            "If your model is not oriented correctly (e.g. object is not facing the Z direction as its forward) then override them here.")]
        [OverrideLabel("Override Model Forward")] public bool forwardAxisOverride;
        [Tooltip("Direction the object is facing in local space.")]
        [ConditionalField(nameof(forwardAxisOverride))] [OrthogonalUnitVector3] public Vector3 forwardAxis = Vector3.forward;

        [Tooltip(
            "If your model is not oriented correctly (e.g. object's side is not facing the X direction as its right) then override them here.")]
        [OverrideLabel("Override Model Right")] public bool rightAxisOverride;
        [Tooltip("Direction the object's side is facing in local space.")]
        [ConditionalField(nameof(rightAxisOverride))] [OrthogonalUnitVector3] public Vector3 rightAxis = Vector3.right;

        [ReadOnly] [SerializeField] private Vector3 velocity;
        private bool _isInteracting;
        private Vector3? _prevSourcePosition;

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

            // Debug.DrawRay(transform.position, transform.forward, Color.blue);
            // Debug.DrawRay(transform.position, transform.right, Color.red);
            // Debug.DrawRay(transform.position, transform.up, Color.green);
            // Debug.DrawRay(transform.position, transform.rotation * movementAxis, Color.yellow);
        }

        public override void OnStartInteract(InteractionData interactionData)
        {
            // base.OnStartInteract(interactionData);
            _prevSourcePosition = null;
        }

        public override void OnInteract(InteractionData interactionData)
        {
            _isInteracting = true;
            // base.OnInteract();

            // transform.forward is the object's forward direction represented in world space (same applies to .right and .up).
            // In local space, the object's forward is always (0,0,1), right always (1,0,0), and up always (0,1,0).
            // i.e. It is = transform.rotation * Vector3.forward
            // Get forward and right directions in world space.
            Vector3 forwardDirection = transform.rotation * (forwardAxisOverride ? forwardAxis : movementAxis);
            Vector3 rightDirection = rightAxisOverride ? transform.rotation * rightAxis : transform.right;

            // Dot products used as multipliers. For example, if the player is facing to the side of a drawer, y-axis mouse movement
            // should contribute 0% to the interaction, but x-axis mouse movement should contribute 100% to the interaction.
            float dotProductRight = Vector3.Dot(rightDirection, interactionData.Source.forward);
            float dotProductForward = Vector3.Dot(forwardDirection, interactionData.Source.forward);

            // Apply source's positional movement velocity to the interactable object.
            if (_prevSourcePosition != null)
            {
                Vector3 distanceMoved = (Vector3)(interactionData.Source.position - _prevSourcePosition);
                velocity += transform.localRotation * movementAxis *
                            (pushStrength * (Vector3.Dot(distanceMoved, interactionData.Source.forward) * dotProductForward / mass +
                                             Vector3.Dot(distanceMoved, interactionData.Source.right) * dotProductRight / mass));
            }

            // localPosition moves the object in parent space.
            // Therefore, multiplying by localRotation will give us a vector in parent space.
            velocity += transform.localRotation * movementAxis * (interactionData.InteractionForce.x / mass * dotProductRight +
                                                                  interactionData.InteractionForce.y / mass * dotProductForward);

            _prevSourcePosition = interactionData.Source.position;
        }

        public override void OnEndInteract(InteractionData interactionData)
        {
            _isInteracting = false;
        }

        private bool ReachedLimit()
        {
            float component = Vector3.Dot(Vector3.Scale(transform.localPosition, movementAxis.normalized), movementAxis.normalized);
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