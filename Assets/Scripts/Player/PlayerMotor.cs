using System;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepDreams.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerMotor : MonoBehaviour
    {
        // ---- Inspector Fields ----
        [Separator("Movement")]
        [SerializeField] [Range(0.0f, 1.0f)] private float moveDamping = 0.2f;
        [SerializeField] private float walkSpeed = 6.0f;
        [SerializeField] private float walkBackSpeedMultiplier = 0.8f;
        [SerializeField] private float walkStride = 1.0f;
        [SerializeField] private float gravity = -9.81f;

        [Separator("Camera")]
        public Transform camTransform;

        // ---- References ----
        public CharacterController Controller { get; private set; }

        // --- Jumping ---
        public bool IsGrounded { get; private set; }

        // --- Movement ---
        public bool IsMoving { get; set; }
        public bool IsMovingForward { get; set; }
        public bool IsMovingBackward { get; set; }
        public bool IsCrouching { get; set; }
        public bool IsRunning { get; set; }
        public float MoveSpeed { get; set; }

        private float _prevPlayerStride;
        private float _playerStride;
        public float PlayerStride
        {
            get => _playerStride;
            set => OnStrideChange(value);
        }
        public float StrideDistance { get; set; }

        private Vector3 _velocity;
        private float _velocityY;

        private Vector2 _targetMoveDir;
        private Vector2 _currentMoveDir;
        private Vector2 _currentMoveDirVelocity;

        private Vector3 _prevTransformPos = Vector3.zero;
        private Vector3 _prevStridePos = Vector3.zero;

        // --- Crouching ---
        public float Height
        {
            get => Controller.height;
            set => Controller.height = value;
        }
        public Vector3 Center
        {
            get => Controller.center;
            set => Controller.center = value;
        }
        public float Radius => Controller.radius;

        // ----- Events -----
        public event Action OnBeforeMove;
        public event Action OnStride;

        private void Awake()
        {
            Controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            MoveSpeed = IsMoving ? walkSpeed : 0.0f;
            PlayerStride = walkStride;

            OnBeforeMove?.Invoke();

            MoveSpeed = IsMovingBackward ? MoveSpeed * walkBackSpeedMultiplier : MoveSpeed;

            MovePlayer();

            HandleStride();
        }

        private void MovePlayer()
        {
            IsGrounded = Controller.isGrounded;

            if (IsGrounded)
            {
                _velocityY = 0.0f;
            }

            Vector2 targetMoveVector = _targetMoveDir * MoveSpeed;

            _currentMoveDir = Vector2.SmoothDamp(_currentMoveDir, targetMoveVector, ref _currentMoveDirVelocity, moveDamping);

            _velocityY += gravity * Time.deltaTime; // acceleration = meters per second **squared**.
            _velocity = transform.forward * _currentMoveDir.y + transform.right * _currentMoveDir.x + Vector3.up * _velocityY;

            _prevTransformPos = transform.position;

            Controller.Move(_velocity * Time.deltaTime);
        }

        private void HandleStride()
        {
            StrideDistance += Vector3.Magnitude(transform.position - _prevTransformPos);

            if (!IsMoving)
            {
                _prevStridePos = transform.position;
                StrideDistance = _playerStride - 0.2f;
            }

            if (StrideDistance < _playerStride)
            {
                return;
            }

            _prevStridePos = transform.position;
            StrideDistance = 0;
            OnStride?.Invoke();
        }

        private void OnStrideChange(float newStride)
        {
            if (Math.Abs(_prevPlayerStride - newStride) < 0.001f)
            {
                return;
            }

            // Keep the same percentage distance to the target from the previous stride to the new stride.
            float amountOfChange = newStride / _playerStride;
            _prevPlayerStride = newStride;
            _playerStride = newStride;
            StrideDistance *= amountOfChange;
        }

        private void OnMove(InputValue value)
        {
            _targetMoveDir = value.Get<Vector2>();
            IsMoving = _targetMoveDir != Vector2.zero;
            IsMovingForward = _targetMoveDir.y > 0.0f;
            IsMovingBackward = _targetMoveDir.y < 0.0f;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_prevStridePos + transform.forward * PlayerStride, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_prevStridePos + transform.forward * PlayerStride * StrideDistance, 0.1f);
        }
    }
}