using MyBox;
using UnityEngine;

namespace DeepDreams.Player
{
    [RequireComponent(typeof(PlayerBlackboard))]
    public class PlayerMotor : MonoBehaviour
    {
        // ---- Inspector Fields ----
        [Separator("Movement")]
        [SerializeField] [Range(0.0f, 1.0f)] private float moveDamping = 0.2f;
        [SerializeField] private float walkSpeed = 6.0f;
        [SerializeField] private float walkBackSpeedMultiplier = 0.8f;
        [SerializeField] private float walkStride = 1.0f;
        [SerializeField] private float gravity = -9.81f;

        private PlayerBlackboard _blackboard;
        private CharacterController _playerController;

        private Vector3 _velocity;
        private float _velocityY;

        private Vector2 _currentMoveDir;
        private Vector2 _currentMoveDirVelocity;

        private void Awake()
        {
            _blackboard = GetComponent<PlayerBlackboard>();
            _playerController = _blackboard._Controller;

            _blackboard.OnPlayerWalk += HandleState;
            _blackboard.MoveSpeed = walkSpeed;
            _blackboard.PlayerStride = walkStride;
        }

        private void OnDestroy()
        {
            _blackboard.OnPlayerWalk -= HandleState;
        }

        private void HandleState()
        {
            _blackboard.MoveSpeed = walkSpeed;
            _blackboard.PlayerStride = walkStride;
        }

        private void Update()
        {
            MovePlayer();
        }

        private void MovePlayer()
        {
            _blackboard.IsGrounded = _playerController.isGrounded;
            if (_blackboard.IsGrounded) _velocityY = 0.0f;

            Vector2 targetMoveVector = _blackboard.targetMoveDir *
                                       (_blackboard.MoveSpeed * (_blackboard.IsMovingBackward ? walkBackSpeedMultiplier : 1.0f));
            _currentMoveDir = Vector2.SmoothDamp(_currentMoveDir, targetMoveVector, ref _currentMoveDirVelocity, moveDamping);

            _velocityY += gravity * Time.deltaTime; // acceleration = meters per second **squared**.
            _velocity = transform.forward * _currentMoveDir.y + transform.right * _currentMoveDir.x + Vector3.up * _velocityY;

            _playerController.Move(_velocity * Time.deltaTime);
        }
    }
}