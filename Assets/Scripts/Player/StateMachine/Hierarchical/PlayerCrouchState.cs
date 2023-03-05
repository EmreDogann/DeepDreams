namespace DeepDreams.Player.StateMachine.Hierarchical
{
    public class PlayerCrouchState : PlayerBaseState
    {
        public PlayerCrouchState(PlayerHierarchicalStateMachine context, PlayerStateFactory playerStateFactory) : base(context,
            playerStateFactory,
            PlayerState.Crouch) {}

        public override void EnterState() {}

        public override void UpdateState()
        {
            if (CheckSwitchState())
            {
                // ReSharper disable once RedundantJumpStatement
                return;
            }
        }

        public override void ExitState() {}

        public override bool CheckSwitchState()
        {
            return false;
        }

        public override void InitializeSubState() {}
    }
}