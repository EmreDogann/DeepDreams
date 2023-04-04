using System;
using DeepDreams.Player.Camera;
using DeepDreams.Player.StateMachine.Simple;
using MyBox;
using UnityEngine;

namespace DeepDreams.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerBlackboard : MonoBehaviour
    {
        [HideInInspector] public Vector2 targetMoveDir;
        [HideInInspector] public CharacterController _Controller;

        // For debugging purposes only.
        [ReadOnly] public PlayerState currentPlayerState;

        // --- Jumping ---
        public bool IsGrounded { get; set; }

        // --- Movement ---
        public bool IsMoving { get; set; }
        public bool IsMovingForward { get; set; }
        public bool IsMovingBackward { get; set; }
        private bool _isCrouching;
        public bool IsCrouching
        {
            get => _isCrouching;
            set
            {
                _isCrouching = value;
                if (!_isCrouching) OnPlayerUncrouch?.Invoke();
            }
        }
        public bool IsCrouchingBlocked { get; set; }
        public bool IsRunning { get; set; }
        public float MoveSpeed { get; set; }

        private float _playerStride;
        public float PlayerStride
        {
            get => _playerStride;
            set
            {
                _playerStride = value;
                OnStrideChange?.Invoke(value);
            }
        }
        public float StrideDistance { get; set; }

        public Action OnStride;
        public Action<float> OnStrideChange;
        public Action OnPlayerWalk;
        public Action<bool> OnPlayerCrouch;
        public Action OnPlayerUncrouch;
        public Action OnPlayerRun;

        public CameraCurveShake movingStopCameraAnimation;

        private void Awake()
        {
            _Controller = GetComponent<CharacterController>();
        }
    }
}