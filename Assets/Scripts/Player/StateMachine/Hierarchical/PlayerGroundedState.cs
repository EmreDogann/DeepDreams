using UnityEngine;

namespace DeepDreams.Player.StateMachine.Hierarchical
{
    public class PlayerGroundedState : PlayerBaseState, IRootState
    {
        public PlayerGroundedState(PlayerHierarchicalStateMachine context, PlayerStateFactory playerStateFactory) : base(context,
            playerStateFactory,
            PlayerState.Grounded) {}

        public void HandleGravity()
        {
            Ctx.AppliedMovementY = 0.0f; // Set Y velocity to 0 when touched the ground.
            Ctx.AppliedMovementY += Ctx.Gravity * Time.deltaTime; // acceleration = meters per second **squared**.
        }

        public override void EnterState()
        {
            InitializeSubState();
        }

        public override void UpdateState()
        {
            if (CheckSwitchState()) return;

            HandleGravity();
        }

        public override void ExitState() {}

        public override void InitializeSubState()
        {
            if (!Ctx.IsMovementPressed && !Ctx.IsRunPressed) SetSubState(Factory.Get(PlayerState.Idle));
            else if (Ctx.IsMovementPressed && !Ctx.IsRunPressed) SetSubState(Factory.Get(PlayerState.Walk));
            else SetSubState(Factory.Get(PlayerState.Run));
        }

        public override bool CheckSwitchState()
        {
            bool isStateSwitched = false;

            if (!Ctx.IsGrounded)
            {
                SwitchState(Factory.Get(PlayerState.Fall));
                isStateSwitched = true;
            }

            return isStateSwitched;
        }
    }
}