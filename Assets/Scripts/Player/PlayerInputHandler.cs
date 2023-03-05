using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepDreams.Player
{
    [RequireComponent(typeof(PlayerBlackboard))]
    public class PlayerInputHandler : MonoBehaviour
    {
        private PlayerBlackboard _blackboard;

        private void Awake()
        {
            _blackboard = GetComponent<PlayerBlackboard>();
        }

        private void OnMove(InputValue value)
        {
            _blackboard.targetMoveDir = value.Get<Vector2>();
            _blackboard.IsMoving = _blackboard.targetMoveDir != Vector2.zero;
            _blackboard.IsMovingForward = _blackboard.targetMoveDir.y > 0.0f;
            _blackboard.IsMovingBackward = _blackboard.targetMoveDir.y < 0.0f;
        }

        private void OnRun(InputValue value)
        {
            _blackboard.IsRunning = value.isPressed;
        }

        private void OnCrouch(InputValue value)
        {
            _blackboard.IsCrouching = value.isPressed;
        }
    }
}