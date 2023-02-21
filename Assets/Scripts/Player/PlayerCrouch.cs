using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepDreams.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    public class PlayerCrouch : MonoBehaviour
    {
        [SerializeField] private float crouchSpeed = 4.0f;
        [SerializeField] private float crouchStride = 0.8f;
        [SerializeField] private float crouchHeight = 1.0f;
        [SerializeField] private float crouchTransitionSpeed = 10.0f;

        private PlayerMotor _playerController;
        private LayerMask _layerMask;
        private RaycastHit _hit;
        private Vector3[] _rayOffsets;

        private bool IsCrouching => _standingHeight - _currentHeight > 0.01f;
        private bool _isCrouchPressed;
        private bool _isBlocked;

        private float _standingHeight;
        private Vector3 _standingCenter;
        private float _currentHeight;
        private Vector3 _initialCameraPosition;

        private void Awake() {
            _playerController = GetComponent<PlayerMotor>();
            _layerMask = LayerMask.GetMask("Environment");
        }

        private void Start() {
            _initialCameraPosition = _playerController.camTransform.localPosition;
            _standingHeight = _currentHeight = _playerController.Height;
            _standingCenter = _playerController.Center;

            _rayOffsets = new Vector3[5];
            _rayOffsets[0] = new Vector3(0, 0, 0); // Center Ray

            _rayOffsets[2] = new Vector3(_playerController.Radius * 0.75f, 0, 0); // Right Ray
            _rayOffsets[1] = new Vector3(-_playerController.Radius * 0.75f, 0, 0); // Left Ray

            _rayOffsets[3] = new Vector3(0, 0, _playerController.Radius * 0.75f); // Front Ray
            _rayOffsets[4] = new Vector3(0, 0, -_playerController.Radius * 0.75f); // Back Ray
        }

        private void OnEnable() {
            _playerController.OnBeforeMove += HandleCrouch;
        }


        private void OnDisable() {
            _playerController.OnBeforeMove -= HandleCrouch;
        }

        private void HandleCrouch() {
            if (_isCrouchPressed || _isBlocked)
            {
                _playerController.MoveSpeed = crouchSpeed;
                _playerController.PlayerStride = crouchStride;
            }

            float heightTarget = _isCrouchPressed ? crouchHeight : _standingHeight;

            if (IsCrouching && !_isCrouchPressed)
            {
                _isBlocked = false;
                foreach (Vector3 ray in _rayOffsets)
                {
                    Vector3 origin = transform.position + ray + new Vector3(0, _currentHeight, 0);
                    if (Physics.Raycast(origin, Vector3.up, out _hit, _standingHeight - _currentHeight, _layerMask))
                    {
                        float distanceToCeiling = _hit.point.y - origin.y;
                        heightTarget = Mathf.Max(_currentHeight + distanceToCeiling - 0.01f, crouchHeight);
                        _isBlocked = true;
                    }
                }
            }

            if (!Mathf.Approximately(heightTarget, _currentHeight))
            {
                float crouchDelta = crouchTransitionSpeed * Time.deltaTime;
                _currentHeight = Mathf.Lerp(_currentHeight, heightTarget, crouchDelta);

                Vector3 halfHeightDifference = new Vector3(0, (_standingHeight - _currentHeight) * 0.5f, 0);
                Vector3 newCameraPosition = _initialCameraPosition - halfHeightDifference * 2.0f;

                _playerController.camTransform.localPosition = newCameraPosition;
                _playerController.Center = _standingCenter - halfHeightDifference;
                _playerController.Height = _currentHeight;
            }
        }

        private void OnCrouch(InputValue value) {
            _isCrouchPressed = value.isPressed;
            _playerController.IsCrouching = _isCrouchPressed;
        }
    }
}