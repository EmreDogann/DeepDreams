using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

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
        public float min;
        public float max;

        private DoorState _doorState;
        private Vector3 velocity;

        public Door(float holdDuration, bool holdInteract, float multipleUse, bool isInteractable) : base(holdDuration, holdInteract,
            multipleUse, isInteractable) {}

        private void Awake()
        {
            _doorState = DoorState.Closed;
        }

        public override void OnInteract(InteractionSource interactionSource)
        {
            // base.OnInteract();

            float dotProductRight = Vector3.Dot(transform.right, interactionSource.direction);
            float dotProductForward = Vector3.Dot(-transform.forward, interactionSource.direction);

            Vector3 newRotation = Vector3.SmoothDamp(transform.localEulerAngles, transform.localEulerAngles + new Vector3(0.0f,
                    Mouse.current.delta.ReadValue().x * dotProductRight + Mouse.current.delta.ReadValue().y * dotProductForward, 0.0f),
                ref velocity, 0.1f);
            newRotation.y = Clamp(newRotation.y, min, max);
            transform.localRotation = Quaternion.Euler(newRotation);
        }

        public override void OnEndInteract()
        {
            velocity = Vector3.zero;
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