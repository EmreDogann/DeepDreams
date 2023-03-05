using System;
using DeepDreams.Player.StateMachine.Simple;
using UnityEngine;

namespace DeepDreams.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerBlackboard : MonoBehaviour
    {
        [HideInInspector] public Vector2 targetMoveDir;
        [HideInInspector] public CharacterController _Controller;

        [HideInInspector] public PlayerState currentPlayerState;

        // --- Jumping ---
        public bool IsGrounded { get; set; }

        // --- Movement ---
        public bool IsMoving { get; set; }
        public bool IsMovingForward { get; set; }
        public bool IsMovingBackward { get; set; }
        public bool IsCrouching { get; set; }
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
        public Action OnPlayerRun;

        private void Awake()
        {
            _Controller = GetComponent<CharacterController>();
        }
    }
}