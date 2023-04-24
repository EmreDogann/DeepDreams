﻿using System;
using MyBox;
using UnityEngine;

namespace DeepDreams.Interactions
{
    public class Door : InteractableBase
    {
        [Separator("General")]
        public float min = -90.0f;
        public float max = 90.0f;
        public float mass = 1.0f;
        public float angularDrag = 0.1f;
        public float stoppingDrag = 0.1f;
        public float pushStrength = 100.0f;

        [Separator("Automatic Closing")]
        [Tooltip("The angle at which or below the door will automatically close.")]
        public float closeThresholdAngle = 10.0f;
        [Tooltip("The speed at which the door will automatically close.")]
        public float closeSpeed = 5.0f;

        [ReadOnly] [SerializeField] private Vector3 velocity;
        private bool _isInteracting;
        private Vector3 _endInteractionAngle;
        private Vector3 _endInteractionVelocity;
        private Vector3? _prevSourcePosition;

        public Door(float holdDuration, bool holdInteract, float multipleUse, bool isInteractable) : base(holdDuration, holdInteract,
            multipleUse, isInteractable) {}

        private void Update()
        {
            Vector3 newRotation = Vector3.zero;

            if (!_isInteracting) velocity *= Mathf.Clamp01(1.0f - stoppingDrag * Time.deltaTime);
            else velocity *= Mathf.Clamp01(1.0f - angularDrag * Time.deltaTime);

            newRotation += velocity * Time.deltaTime;
            newRotation += transform.localEulerAngles;
            newRotation.y = Clamp(newRotation.y, min, max);
            transform.localRotation = Quaternion.Euler(newRotation);

            if (!_isInteracting && Math.Abs(AngleRemap(transform.localEulerAngles.y, 180.0f)) <= closeThresholdAngle)
            {
                // Move our position a step closer to the target.
                float step = closeSpeed * Time.deltaTime; // calculate distance to move
                Vector3 targetAngle = transform.localEulerAngles;
                targetAngle.y = 0.0f;
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(targetAngle), step);
            }
        }

        public override void OnStartInteract(InteractionData interactionData)
        {
            // base.OnStartInteract();
            _prevSourcePosition = null;
        }

        public override void OnInteract(InteractionData interactionData)
        {
            _isInteracting = true;
            // base.OnInteract();

            // Dot products used as multipliers. For example, if the player is facing to the side of a drawer, y-axis mouse movement
            // should contribute 0% to the interaction, but x-axis mouse movement should contribute 100% to the interaction.
            float dotProductRight = Vector3.Dot(transform.right, interactionData.Source.forward);
            float dotProductForward = Vector3.Dot(-transform.forward, interactionData.Source.forward);

            // Apply source's positional movement velocity to the interactable object.
            if (_prevSourcePosition != null)
            {
                Vector3 distanceMoved = (Vector3)(interactionData.Source.position - _prevSourcePosition);
                velocity.y += pushStrength *
                              (Vector3.Dot(distanceMoved, interactionData.Source.forward) * dotProductForward / mass +
                               Vector3.Dot(distanceMoved, interactionData.Source.right) * dotProductRight / mass);
            }

            velocity += new Vector3(0.0f,
                interactionData.InteractionForce.x / mass * dotProductRight + interactionData.InteractionForce.y / mass * dotProductForward,
                0.0f);

            _prevSourcePosition = interactionData.Source.position;
        }

        public override void OnEndInteract(InteractionData interactionData)
        {
            _isInteracting = false;
        }

        // From: http://answers.unity.com/comments/1406762/view.html
        // Expects angle in the range 0 to 360
        // Expects min and max in the range -180 to 180
        // Returns the clamped angle in the range 0 to 360
        private float Clamp(float angle, float min, float max)
        {
            if (angle > 180.0f) // remap 0 - 360 --> -180 - 180
                angle -= 360.0f;
            angle = Mathf.Clamp(angle, min, max);
            if (angle < 0.0f) // map back to 0 - 360
                angle += 360.0f;
            return angle;
        }

        private float AngleRemap(float angle, float max)
        {
            if (angle > max) angle -= 360.0f;

            return angle;
        }
    }
}