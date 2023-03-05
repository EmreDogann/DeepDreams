using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepDreams.Player.StateMachine.Hierarchical
{
    public class PlayerHierarchicalStateMachine : MonoBehaviour
    {
        #region ----- Inspector Fields -----

        [Header("General")]
        [SerializeField]
        private float walkSpeed = 6.0f;
        [SerializeField] private float runSpeed = 10.0f;

        [SerializeField] [Range(0.0f, 1.0f)] private float moveDirectionSmoothTime = 0.2f;
        [SerializeField] [Range(0.0f, 1.0f)] private float moveSpeedSmoothTime = 0.2f;

        [SerializeField] private Vector2 lookSpeed = new Vector2(6.0f, 6.0f);
        [SerializeField] [Range(0.0f, 1.0f)] private float mouseSmoothTime = 0.03f;

        // ----- Jumping -----
        [Header("Air")]
        [SerializeField]
        private float gravity = -9.81f;

        // ---- Camera ----
        [Header("Camera")]
        [SerializeField]
        private Transform camRotater;

        #endregion

        // ---- Input ----
        public Transform PlayerTransform { get; private set; }
        public CharacterController CharacterController { get; private set; }
        private PlayerInput _playerInput;
        private InputAction _moveAction;

        // ----- State Machine -----
        public PlayerBaseState CurrentState { get; set; }
        private PlayerStateFactory _states;

        #region ---- Public Properties ----

        public bool IsGrounded { get; private set; }
        public bool IsMovementPressed { get; private set; }
        public bool IsRunPressed { get; private set; }
        public Vector3 AppliedMovement { get; set; }
        public float AppliedMovementY { get; set; }
        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;
        public float Gravity => gravity;

        #endregion

        // ----- Movement -----
        public Vector2 CurrentMoveDir { get; private set; }
        private Vector2 _targetMoveDir;
        private Vector2 _currentMoveDirVelocity;

        public float CurrentMoveSpeed { get; set; }
        public float TargetMoveSpeed { get; set; }
        private float _currentMoveSpeedVelocity;

        public Vector2 LookDirection { get; set; }
        private Vector2 _currentMouseDelta;
        private Vector2 _currentMouseDeltaVelocity;

        private void OnEnable()
        {
            _moveAction.performed += MoveInput;
            _moveAction.canceled += MoveInput;
        }

        private void OnDisable()
        {
            _moveAction.performed -= MoveInput;
            _moveAction.canceled -= MoveInput;
        }

        private void Awake()
        {
            PlayerTransform = transform;

            _states = new PlayerStateFactory(this);
            CurrentState = _states.Get(PlayerState.Grounded);
            CurrentState.EnterState();

            CharacterController = GetComponent<CharacterController>();

            _playerInput = GetComponent<PlayerInput>();
            _moveAction = _playerInput.actions["Move"];

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            // Rotate object to face direction of camera view.
            CameraLook();

            // Update player's input movement variables
            HandleMove();

            // Update current state
            CurrentState.UpdateStates();

            // Move the player in direction of camera view.
            CharacterController.Move(AppliedMovement * Time.deltaTime);
        }

        private void HandleMove()
        {
            IsGrounded = CharacterController.isGrounded;

            CurrentMoveDir = Vector2.SmoothDamp(CurrentMoveDir, _targetMoveDir, ref _currentMoveDirVelocity, moveDirectionSmoothTime);
            CurrentMoveSpeed = Mathf.SmoothDamp(CurrentMoveSpeed, TargetMoveSpeed, ref _currentMoveSpeedVelocity, moveSpeedSmoothTime);

            AppliedMovement = (transform.forward * CurrentMoveDir.y + transform.right * CurrentMoveDir.x) * CurrentMoveSpeed +
                              Vector3.up * AppliedMovementY;
        }

        private void CameraLook()
        {
            transform.localEulerAngles = Vector3.up * LookDirection.y;
            camRotater.localEulerAngles = Vector3.right * LookDirection.x;
        }

        private void OnLook(InputValue value)
        {
            Vector2 targetMouseDelta = value.Get<Vector2>();
            _currentMouseDelta = Vector2.SmoothDamp(_currentMouseDelta, targetMouseDelta, ref _currentMouseDeltaVelocity, mouseSmoothTime);

            Vector2 tempLookDir = LookDirection;
            tempLookDir.y += _currentMouseDelta.x * lookSpeed.x * 0.01f;
            tempLookDir.x -= _currentMouseDelta.y * lookSpeed.y * 0.01f;
            tempLookDir.x = Mathf.Clamp(tempLookDir.x, -90.0f, 90.0f);

            LookDirection = tempLookDir;
        }

        private void MoveInput(InputAction.CallbackContext context)
        {
            _targetMoveDir = context.ReadValue<Vector2>();
            if (context.started || context.performed) IsMovementPressed = true;
            else IsMovementPressed = false;
        }

        private void OnRun(InputValue value)
        {
            if (value.isPressed) IsRunPressed = true;
            else IsRunPressed = false;
        }
    }
}