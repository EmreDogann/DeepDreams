using System.Collections.Generic;
using UnityEngine;

namespace DeepDreams.ThirdPartyAssets.GG_Camera_Shake.Runtime
{
    /// <summary>
    ///     Camera shaker component registeres new shakes, holds a list of active shakes, and applies them to the camera additively.
    /// </summary>
    public class CameraShaker : MonoBehaviour
    {
        public static CameraShaker Instance;
        public static CameraShakePresets Presets;

        private readonly List<ICameraShake> activeShakes = new List<ICameraShake>();

        [Tooltip(
            "Transform which will be affected by the shakes.\n\nCameraShaker will set this transform's local position and rotation.")]
        [SerializeField]
        private Transform cameraTransform;


        [Tooltip("Scales the strength of all shakes.")]
        [Range(0, 2)]
        [SerializeField]
        public float StrengthMultiplier = 1;

        public CameraShakePresets ShakePresets;

        public void SetShakeStrengthPercentage(float strengthPercent)
        {
            StrengthMultiplier = strengthPercent / 100.0f;
        }

        /// <summary>
        ///     Adds a shake to the list of active shakes.
        /// </summary>
        public static void Shake(ICameraShake shake)
        {
            if (IsInstanceNull())
            {
                return;
            }

            Instance.RegisterShake(shake);
        }

        /// <summary>
        ///     Check if specified shake exists in the list of active shakes.
        /// </summary>
        public static bool Contains(ICameraShake shake)
        {
            return Instance.activeShakes.Contains(shake);
        }

        /// <summary>
        ///     Adds a shake to the list of active shakes.
        /// </summary>
        public void RegisterShake(ICameraShake shake)
        {
            shake.Initialize(cameraTransform.position,
                cameraTransform.rotation);

            activeShakes.Add(shake);
        }

        /// <summary>
        ///     Sets the transform which will be affected by the shakes.
        /// </summary>
        public void SetCameraTransform(Transform cameraTransform)
        {
            cameraTransform.localPosition = Vector3.zero;
            cameraTransform.localEulerAngles = Vector3.zero;
            this.cameraTransform = cameraTransform;
        }

        private void Awake()
        {
            Instance = this;
            ShakePresets = new CameraShakePresets(this);
            Presets = ShakePresets;

            if (cameraTransform == null)
            {
                cameraTransform = transform;
            }
        }

        private void Update()
        {
            if (cameraTransform == null)
            {
                return;
            }

            Displacement cameraDisplacement = Displacement.Zero;

            for (int i = activeShakes.Count - 1; i >= 0; i--)
            {
                if (activeShakes[i].IsFinished)
                {
                    activeShakes.RemoveAt(i);
                }
                else
                {
                    activeShakes[i].UpdateShake(Time.deltaTime, cameraTransform.position, cameraTransform.rotation);
                    cameraDisplacement += activeShakes[i].CurrentDisplacement;
                }
            }

            cameraTransform.localPosition = StrengthMultiplier * cameraDisplacement.position;
            cameraTransform.localRotation = Quaternion.Euler(StrengthMultiplier * cameraDisplacement.eulerAngles);
        }

        private static bool IsInstanceNull()
        {
            if (Instance == null)
            {
                Debug.LogError("CameraShaker Instance is missing!");
                return true;
            }

            return false;
        }
    }
}