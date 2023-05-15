using DeepDreams.SaveLoad;
using DeepDreams.SaveLoad.Data;
using MyBox;
using UnityEngine;

namespace DeepDreams.Player
{
    [RequireComponent(typeof(PlayerBlackboard))]
    public class PlayerMotor : MonoBehaviour, ISaveable<GameData>
    {
        // ---- Inspector Fields ----
        [Separator("Movement")]
        [SerializeField] [Range(0.0f, 1.0f)] private float moveDamping = 0.2f;
        [SerializeField] private float walkSpeed = 6.0f;
        [SerializeField] private float walkBackSpeedMultiplier = 0.8f;
        [SerializeField] private float walkStride = 1.0f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundedCheckRadius = 0.2f;
        [SerializeField] private float edgeSlideSpeed = 5.0f;

        private PlayerBlackboard _blackboard;
        private CharacterController _playerController;

        private Vector3 _velocity;
        private float _velocityY;

        private Vector2 _currentMoveDir;
        private Vector2 _currentMoveDirVelocity;

        private Vector3 _edgeSlideMovement;
        private Vector3 _edgeHitPoint;
        private bool _wasGrounded;

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
            _wasGrounded = _blackboard.IsGrounded;
            _blackboard.IsGrounded = CheckGrounded();

            if (!_blackboard.IsGrounded && _wasGrounded)
            {
                _velocityY = 0.0f;
            }

            Vector2 targetMoveVector = _blackboard.targetMoveDir *
                                       (_blackboard.MoveSpeed *
                                        (_blackboard.IsMovingBackward ? walkBackSpeedMultiplier : 1.0f));
            _currentMoveDir = Vector2.SmoothDamp(_currentMoveDir, targetMoveVector, ref _currentMoveDirVelocity,
                moveDamping);

            _velocityY += gravity * Time.deltaTime; // acceleration = meters per second **squared**.
            _velocity = transform.forward * _currentMoveDir.y + transform.right * _currentMoveDir.x +
                        Vector3.up * _velocityY;
            _velocity += _edgeSlideMovement;

            _playerController.Move(_velocity * Time.deltaTime);
        }

        private bool CheckGrounded()
        {
            float floorDistanceFromFoot = _playerController.stepOffset;
            _edgeSlideMovement = Vector2.zero;

            // Check if grounded with edge tolerance.
            if (Physics.SphereCast(transform.position + Vector3.up * (groundedCheckRadius + 0.01f), groundedCheckRadius,
                    Vector3.down,
                    out RaycastHit _, groundedCheckRadius + 0.03f))
            {
                // From: https://forum.unity.com/threads/charactercontroller-and-walking-down-a-stairs.101859/
                // Fixes isGrounded not working when going down stairs/slopes when setting y velocity to 0.
                _velocityY = -_playerController.stepOffset / Time.unscaledDeltaTime;

                return true;
            }

            // If the player collider is floating off but still touching the edge, apply a movement vector to slide the player off the edge.
            if (_playerController.isGrounded)
            {
                Vector3 edgeMovement = transform.position - _edgeHitPoint;
                edgeMovement.y = 0.0f;

                _edgeSlideMovement = edgeMovement.normalized * edgeSlideSpeed;
            }

            return false;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            _edgeHitPoint = hit.point;
        }

        public void SaveData(GameData saveData)
        {
            saveData.playerPosition = transform.position;
        }

        public void LoadData(GameData saveData)
        {
            transform.position = saveData.playerPosition;
        }
    }
}