using MyBox;
using UnityEngine;

namespace DeepDreams.Interactions
{
    public class Door : InteractableBase
    {
        private enum DoorState
        {
            Closed,
            Peek,
            Open,
            Locked
        }

        [Separator("General")]
        public float min = -90.0f;
        public float max = 90.0f;
        public float mass = 1.0f;
        public float drag = 0.1f;

        private DoorState _doorState;
        [ReadOnly] [SerializeField] private Vector3 velocity;
        private bool _isInteracting;
        private Vector3 _endInteractionAngle;
        private Vector3 _endInteractionVelocity;

        public Door(float holdDuration, bool holdInteract, float multipleUse, bool isInteractable) : base(holdDuration, holdInteract,
            multipleUse, isInteractable) {}

        private void Awake()
        {
            _doorState = DoorState.Closed;
        }

        private void Update()
        {
            Vector3 newRotation = Vector3.zero;

            velocity *= Mathf.Clamp01(1.0f - drag * Time.deltaTime);
            newRotation += velocity * Time.deltaTime;
            newRotation += transform.localEulerAngles;
            newRotation.y = Clamp(newRotation.y, min, max);
            transform.localRotation = Quaternion.Euler(newRotation);
        }

        public override void OnInteract(InteractionData interactionData)
        {
            _isInteracting = true;
            // base.OnInteract();

            float dotProductRight = Vector3.Dot(transform.right, interactionData.SourceDirection);
            float dotProductForward = Vector3.Dot(-transform.forward, interactionData.SourceDirection);

            velocity += new Vector3(0.0f,
                interactionData.InteractionForce.x / mass * dotProductRight + interactionData.InteractionForce.y / mass * dotProductForward,
                0.0f);
        }

        public override void OnEndInteract()
        {
            _isInteracting = false;
        }

        // From: http://answers.unity.com/comments/1406762/view.html
        // Expects angle in the range 0 to 360
        // Expects min and max in the range -180 to 180
        // Returns the clamped angle in the range 0 to 360
        private float Clamp(float angle, float min, float max)
        {
            if (angle > 180f) // remap 0 - 360 --> -180 - 180
                angle -= 360f;
            angle = Mathf.Clamp(angle, min, max);
            if (angle < 0f) // map back to 0 - 360
                angle += 360f;
            return angle;
        }
    }
}