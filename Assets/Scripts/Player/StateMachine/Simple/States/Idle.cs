using DeepDreams.ThirdPartyAssets.GG_Camera_Shake.Runtime;

namespace DeepDreams.Player.StateMachine.Simple.States
{
    public class Idle : IState
    {
        private readonly PlayerStateMachine _stateMachine;
        private readonly PlayerBlackboard _blackboard;

        public Idle(PlayerStateMachine stateMachine, PlayerBlackboard blackboard)
        {
            _stateMachine = stateMachine;
            _blackboard = blackboard;
        }

        public PlayerState GetStateType()
        {
            return PlayerState.Idle;
        }

        public void Tick()
        {
            // Debug.Log("Idle Update");
        }

        public void OnEnter()
        {
            // Debug.Log("Idle Enter");
            _blackboard.MoveSpeed = 0.0f;

            PlayerState previousState = _stateMachine.GetPreviousStateType();

            if (previousState == PlayerState.Walking || previousState == PlayerState.Running)
            {
                if (_blackboard.StrideDistance > _blackboard.PlayerStride * 0.4f) _blackboard.OnStride.Invoke();

                if (previousState == PlayerState.Running)
                {
                    if (!CameraShaker.Contains(_blackboard.movingStopCameraAnimation))
                        CameraShaker.Shake(_blackboard.movingStopCameraAnimation);
                    else _blackboard.movingStopCameraAnimation.Initialize();
                }
            }
        }

        public void OnExit()
        {
            // Debug.Log("Idle Exit");
        }
    }
}