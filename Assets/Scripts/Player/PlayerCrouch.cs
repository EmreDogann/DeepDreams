using System.Collections;
using DeepDreams.Audio;
using DeepDreams.Player.Camera;
using DeepDreams.ScriptableObjects.Audio;
using DeepDreams.ThirdPartyAssets.GG_Camera_Shake.Runtime;
using MyBox;
using UnityEngine;

namespace DeepDreams.Player
{
    [RequireComponent(typeof(PlayerBlackboard))]
    public class PlayerCrouch : MonoBehaviour
    {
        [SerializeField] private float crouchMovementSpeed = 4.0f;
        [SerializeField] private float crouchStride = 0.8f;
        [SerializeField] private float crouchHeight = 1.0f;
        [SerializeField] private float crouchDuration = 0.13f;

        [SerializeField] private Transform cameraHolder;

        [Separator("Audio")]
        [SerializeField] private AudioReference crouchSound;

        [Separator("Animation")]
        [SerializeField] private CameraCurveShake cameraCrouchAnimation;

        [Separator("Collision Settings")]
        [SerializeField] private LayerMask collisionDetectionLayerMask;
        [SerializeField] private float crouchCollisionRadius;
        [SerializeField] private float collisionDetectionCooldown;

        private PlayerBlackboard _blackboard;
        private CharacterController _playerController;
        private Coroutine _checkCollisionCoroutine;
        private Coroutine _crouchCoroutine;
        private WaitForSeconds _waitForSeconds;

        private RaycastHit _hit;

        private bool IsNotStanding => _standingHeight - _currentHeight > 0.01f;

        private float _standingHeight;
        private float _heightTarget;
        private Vector3 _standingCenter;
        private float _currentHeight;
        private Vector3 _initialCameraPosition;
        private float _crouchingVelocity;

        private void Awake()
        {
            _blackboard = GetComponent<PlayerBlackboard>();
            _playerController = _blackboard._Controller;

            _blackboard.OnPlayerCrouch += HandleCrouch;

            _waitForSeconds = new WaitForSeconds(collisionDetectionCooldown);
        }

        private void Start()
        {
            _initialCameraPosition = cameraHolder.localPosition;
            _standingHeight = _currentHeight = _playerController.height;
            _standingCenter = _playerController.center;
        }

        private void OnDestroy()
        {
            _blackboard.OnPlayerCrouch -= HandleCrouch;
        }

        private void HandleCrouch(bool toggleOn)
        {
            if (toggleOn)
            {
                _blackboard.MoveSpeed = crouchMovementSpeed;
                _blackboard.PlayerStride = crouchStride;

                cameraCrouchAnimation.Invert(1);
            }
            else cameraCrouchAnimation.Invert(-1);

            if (_checkCollisionCoroutine != null) StopCoroutine(_checkCollisionCoroutine);
            if (_crouchCoroutine != null) StopCoroutine(_crouchCoroutine);
            _crouchCoroutine = StartCoroutine(PerformCrouch(toggleOn));

            AudioManager.instance.PlayOneShot(crouchSound);
        }

        private IEnumerator PerformCrouch(bool toggleOn)
        {
            float finalHeightTarget = toggleOn ? crouchHeight : _standingHeight;
            _heightTarget = finalHeightTarget;

            bool crouchAnimStarted = false;

            // While we have not reached the target height...
            while (!Mathf.Approximately(finalHeightTarget, _currentHeight))
            {
                if (IsNotStanding && !_blackboard.IsCrouching)
                {
                    if (_checkCollisionCoroutine != null) StopCoroutine(_checkCollisionCoroutine);
                    _checkCollisionCoroutine = StartCoroutine(CheckHeightClearance());
                }

                // float crouchDelta = crouchTransitionSpeed * Time.deltaTime;
                // _currentHeight = Mathf.Lerp(_currentHeight, _heightTarget, crouchDelta);
                // float crouchDelta = 1 / 0.3f * Time.deltaTime;
                // _currentHeight = Mathf.MoveTowards(_currentHeight, _heightTarget, crouchDelta);
                _currentHeight = Mathf.SmoothDamp(_currentHeight, _heightTarget, ref _crouchingVelocity, crouchDuration);

                SetPlayerHeight(_currentHeight);

                if (Mathf.Abs(finalHeightTarget - _currentHeight) < 0.7f && !crouchAnimStarted)
                {
                    crouchAnimStarted = true;
                    if (!CameraShaker.Contains(cameraCrouchAnimation)) CameraShaker.Shake(cameraCrouchAnimation);
                    else cameraCrouchAnimation.Initialize();
                }

                yield return null;
            }

            SetPlayerHeight(_heightTarget);

            _blackboard.IsCrouchingBlocked = false;
        }

        private void SetPlayerHeight(float height)
        {
            Vector3 halfHeightDifference = new Vector3(0, (_standingHeight - height) * 0.5f, 0);
            Vector3 newCameraPosition = _initialCameraPosition - halfHeightDifference * 2.0f;

            cameraHolder.localPosition = newCameraPosition;
            _playerController.center = _standingCenter - halfHeightDifference;
            _playerController.height = height;
        }

        private IEnumerator CheckHeightClearance()
        {
            while (IsNotStanding)
            {
                Vector3 origin = transform.position + new Vector3(0, _currentHeight - crouchCollisionRadius, 0);

                // DebugExtension.DebugWireSphere(origin, Color.red, crouchCollisionRadius, collisionDetectionCooldown);
                // Debug.DrawLine(origin, origin + Vector3.up * (_standingHeight - _currentHeight), Color.white);

                if (Physics.SphereCast(origin, crouchCollisionRadius, Vector3.up, out _hit, _standingHeight - _currentHeight,
                        collisionDetectionLayerMask))
                {
                    float distanceToCeiling = _hit.point.y - crouchCollisionRadius - origin.y;
                    _heightTarget = Mathf.Max(_currentHeight + distanceToCeiling - 0.01f, crouchHeight);
                    _blackboard.IsCrouchingBlocked = true;

                    // Vector3 targetPosDebug = origin + Vector3.up * distanceToCeiling;
                    // DebugExtension.DebugWireSphere(targetPosDebug, Color.green, crouchCollisionRadius, collisionDetectionCooldown);
                }
                else _blackboard.IsCrouchingBlocked = false;

                yield return _waitForSeconds;
            }
        }
    }
}