using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepDreams.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    public class PlayerRunning : MonoBehaviour
    {
        [SerializeField] private float runSpeed = 6.0f;
        [SerializeField] private float runStride = 1.2f;

        private PlayerMotor _playerController;

        private bool _isRunning;

        private void Awake() {
            _playerController = GetComponent<PlayerMotor>();
        }

        private void OnEnable() {
            _playerController.OnBeforeMove += HandleSprint;
        }

        private void OnDisable() {
            _playerController.OnBeforeMove -= HandleSprint;
        }

        private void HandleSprint() {
            if (_isRunning && _playerController.IsMovingForward)
            {
                _playerController.MoveSpeed = runSpeed;
                _playerController.PlayerStride = runStride;
            }
        }

        private void OnRun(InputValue value) {
            _isRunning = value.isPressed;
            _playerController.IsRunning = _isRunning;
        }
    }
}