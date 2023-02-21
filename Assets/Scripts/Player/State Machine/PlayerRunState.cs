namespace DeepDreams.Player.State_Machine
{
    public class PlayerRunState : PlayerBaseState
    {
        public PlayerRunState(PlayerStateMachine context, PlayerStateFactory playerStateFactory) : base(context, playerStateFactory,
            PlayerState.Run) {}

        public override void EnterState() {
            Ctx.TargetMoveSpeed = Ctx.RunSpeed;
        }

        public override void UpdateState() {
            if (CheckSwitchState())
            {
                // ReSharper disable once RedundantJumpStatement
                return;
            }
        }

        public override void ExitState() {}

        public override bool CheckSwitchState() {
            bool isStateSwitched = false;
            if (!Ctx.IsMovementPressed)
            {
                SwitchState(Factory.Get(PlayerState.Idle));
                isStateSwitched = true;
            }
            else if (Ctx.IsMovementPressed && !Ctx.IsRunPressed)
            {
                SwitchState(Factory.Get(PlayerState.Walk));
                isStateSwitched = true;
            }

            return isStateSwitched;
        }

        public override void InitializeSubState() {}
    }
}