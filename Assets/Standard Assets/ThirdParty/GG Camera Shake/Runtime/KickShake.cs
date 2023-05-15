using System;
using UnityEngine;

namespace DeepDreams.ThirdPartyAssets.GG_Camera_Shake.Runtime
{
    public class KickShake : ICameraShake
    {
        private readonly Params pars;
        private readonly Vector3? sourcePosition;
        private readonly bool attenuateStrength;

        private Displacement direction;
        private Displacement prevWaypoint;
        private Displacement currentWaypoint;
        private bool release;
        private float t;

        /// <summary>
        ///     Creates an instance of KickShake in the direction from the source to the camera.
        /// </summary>
        /// <param name="parameters">Parameters of the shake.</param>
        /// <param name="sourcePosition">World position of the source of the shake.</param>
        /// <param name="attenuateStrength">Change strength depending on distance from the camera?</param>
        public KickShake(Params parameters, Vector3 sourcePosition, bool attenuateStrength)
        {
            pars = parameters;
            this.sourcePosition = sourcePosition;
            this.attenuateStrength = attenuateStrength;
        }

        /// <summary>
        ///     Creates an instance of KickShake.
        /// </summary>
        /// <param name="parameters">Parameters of the shake.</param>
        /// <param name="direction">Direction of the kick.</param>
        public KickShake(Params parameters, Displacement direction)
        {
            pars = parameters;
            this.direction = direction.Normalized;
        }

        public Displacement CurrentDisplacement { get; private set; }
        public bool IsFinished { get; private set; }


        public void Initialize(Vector3 cameraPosition, Quaternion cameraRotation)
        {
            if (sourcePosition != null)
            {
                direction = Attenuator.Direction(sourcePosition.Value, cameraPosition, cameraRotation);
                if (attenuateStrength)
                    direction *= Attenuator.Strength(pars.attenuation, sourcePosition.Value, cameraPosition);
            }

            currentWaypoint = Displacement.Scale(direction, pars.strength);
        }

        public void UpdateShake(float deltaTime, Vector3 cameraPosition, Quaternion cameraRotation)
        {
            if (t < 1)
            {
                Move(deltaTime,
                    release ? pars.releaseTime : pars.attackTime,
                    release ? pars.releaseCurve : pars.attackCurve);
            }
            else
            {
                CurrentDisplacement = currentWaypoint;
                prevWaypoint = currentWaypoint;

                if (release)
                {
                    IsFinished = true;
                    return;
                }

                release = true;
                t = 0;
                currentWaypoint = Displacement.Zero;
            }
        }

        private void Move(float deltaTime, float duration, AnimationCurve curve)
        {
            if (duration > 0)
                t += deltaTime / duration;
            else
                t = 1;
            CurrentDisplacement = Displacement.Lerp(prevWaypoint, currentWaypoint, curve.Evaluate(t));
        }

        [Serializable]
        public class Params
        {
            /// <summary>
            ///     Strength of the shake for each axis.
            /// </summary>
            [Tooltip("Strength of the shake for each axis.")]
            public Displacement strength = new Displacement(Vector3.zero, Vector3.one);

            /// <summary>
            ///     How long it takes to move forward.
            /// </summary>
            [Tooltip("How long it takes to move forward.")]
            public float attackTime = 0.05f;
            public AnimationCurve attackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            /// <summary>
            ///     How long it takes to move back.
            /// </summary>
            [Tooltip("How long it takes to move back.")]
            public float releaseTime = 0.2f;
            public AnimationCurve releaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            /// <summary>
            ///     How strength falls with distance from the shake source.
            /// </summary>
            [Tooltip("How strength falls with distance from the shake source.")]
            public Attenuator.StrengthAttenuationParams attenuation;
        }
    }
}