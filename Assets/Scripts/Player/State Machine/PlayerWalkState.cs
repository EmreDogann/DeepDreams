namespace DeepDreams.Player.State_Machine
{
    public class PlayerWalkState : PlayerBaseState
    {
        public PlayerWalkState(PlayerStateMachine context, PlayerStateFactory playerStateFactory) : base(context, playerStateFactory,
            PlayerState.Walk) {}

        public override void EnterState() {
            Ctx.TargetMoveSpeed = Ctx.WalkSpeed;
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
            else if (Ctx.IsMovementPressed && Ctx.IsRunPressed)
            {
                SwitchState(Factory.Get(PlayerState.Run));
                isStateSwitched = true;
            }

            return isStateSwitched;
        }

        public override void InitializeSubState() {}
    }
}