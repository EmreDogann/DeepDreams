using System.Collections;
using MyBox;
using UnityEngine;

namespace DeepDreams.Player
{
    [RequireComponent(typeof(PlayerBlackboard))]
    public class PlayerCrouch : MonoBehaviour
    {
        [SerializeField] private float crouchSpeed = 4.0f;
        [SerializeField] private float crouchStride = 0.8f;
        [SerializeField] private float crouchHeight = 1.0f;
        [SerializeField] private float crouchTransitionSpeed = 10.0f;

        [SerializeField] private Transform cameraHolder;

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
                _blackboard.MoveSpeed = crouchSpeed;
                _blackboard.PlayerStride = crouchStride;
            }

            if (_checkCollisionCoroutine != null) StopCoroutine(_checkCollisionCoroutine);
            if (_crouchCoroutine != null) StopCoroutine(_crouchCoroutine);
            _crouchCoroutine = StartCoroutine(PerformCrouch(toggleOn));
        }

        private IEnumerator PerformCrouch(bool toggleOn)
        {
            float finalHeightTarget = toggleOn ? crouchHeight : _standingHeight;
            _heightTarget = finalHeightTarget;

            // While we have not reached the target height...
            while (!Mathf.Approximately(finalHeightTarget, _currentHeight))
            {
                if (IsNotStanding && !_blackboard.IsCrouching)
                {
                    if (_checkCollisionCoroutine != null) StopCoroutine(_checkCollisionCoroutine);
                    _checkCollisionCoroutine = StartCoroutine(CheckHeightClearance());
                }

                float crouchDelta = crouchTransitionSpeed * Time.deltaTime;
                _currentHeight = Mathf.Lerp(_currentHeight, _heightTarget, crouchDelta);

                Vector3 halfHeightDifference = new Vector3(0, (_standingHeight - _currentHeight) * 0.5f, 0);
                Vector3 newCameraPosition = _initialCameraPosition - halfHeightDifference * 2.0f;

                cameraHolder.localPosition = newCameraPosition;
                _playerController.center = _standingCenter - halfHeightDifference;
                _playerController.height = _currentHeight;
                yield return null;
            }

            _blackboard.IsCrouchingBlocked = false;
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