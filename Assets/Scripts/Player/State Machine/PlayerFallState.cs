using UnityEngine;

namespace DeepDreams.Player.State_Machine
{
    public class PlayerFallState : PlayerBaseState, IRootState
    {
        public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory,
            PlayerState.Fall) {}

        public void HandleGravity() {
            Ctx.AppliedMovementY += Ctx.Gravity * Time.deltaTime; // acceleration = meters per second **squared**.
        }

        public override void EnterState() {
            InitializeSubState();
        }

        public override void UpdateState() {
            if (CheckSwitchState())
            {
                // ReSharper disable once RedundantJumpStatement
                return;
            }

            HandleGravity();
        }

        public override void ExitState() {}

        public override bool CheckSwitchState() {
            bool isStateSwitched = false;
            if (Ctx.IsGrounded)
            {
                SwitchState(Factory.Get(PlayerState.Grounded));
                isStateSwitched = true;
            }

            return isStateSwitched;
        }

        public override void InitializeSubState() {
            if (!Ctx.IsMovementPressed && !Ctx.IsRunPressed)
            {
                SetSubState(Factory.Get(PlayerState.Idle));
            }
            else if (Ctx.IsMovementPressed && !Ctx.IsRunPressed)
            {
                SetSubState(Factory.Get(PlayerState.Walk));
            }
            else
            {
                SetSubState(Factory.Get(PlayerState.Run));
            }
        }
    }
}