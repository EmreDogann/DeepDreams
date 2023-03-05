namespace DeepDreams.Player.StateMachine.Hierarchical
{
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(PlayerHierarchicalStateMachine context, PlayerStateFactory playerStateFactory) : base(context,
            playerStateFactory,
            PlayerState.Idle) {}

        public override void EnterState()
        {
            // Ctx.MoveSpeed = 0.0f;
        }

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
            bool isStateSwitched = false;

            if (Ctx.IsMovementPressed && Ctx.IsRunPressed)
            {
                SwitchState(Factory.Get(PlayerState.Run));
                isStateSwitched = true;
            }
            else if (Ctx.IsMovementPressed)
            {
                SwitchState(Factory.Get(PlayerState.Walk));
                isStateSwitched = true;
            }

            return isStateSwitched;
        }

        public override void InitializeSubState() {}
    }
}