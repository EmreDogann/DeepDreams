using UnityEngine;

namespace DeepDreams.Player
{
    [RequireComponent(typeof(PlayerBlackboard))]
    public class PlayerRunning : MonoBehaviour
    {
        [SerializeField] private float runSpeed = 6.0f;
        [SerializeField] private float runStride = 1.2f;

        private PlayerBlackboard _blackboard;

        private void Awake()
        {
            _blackboard = GetComponent<PlayerBlackboard>();
            _blackboard.OnPlayerRun += HandleSprint;
        }

        private void OnDestroy()
        {
            _blackboard.OnPlayerRun -= HandleSprint;
        }

        private void HandleSprint()
        {
            if (_blackboard.IsRunning && _blackboard.IsMovingForward)
            {
                _blackboard.MoveSpeed = runSpeed;
                _blackboard.PlayerStride = runStride;
            }
        }
    }
}