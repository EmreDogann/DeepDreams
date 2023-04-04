using System;
using DeepDreams.Player.StateMachine.Simple.States;
using UnityEngine;

namespace DeepDreams.Player.StateMachine.Simple
{
    public enum PlayerState
    {
        Idle,
        Walking,
        Running,
        Falling,
        CrouchIdle,
        CrouchWalk
    }

    [RequireComponent(typeof(PlayerBlackboard))]
    public class PlayerStateMachine : MonoBehaviour
    {
        private StateMachine _stateMachine;
        private PlayerBlackboard _blackboard;

        private void Awake()
        {
            _stateMachine = new StateMachine();
            _blackboard = GetComponent<PlayerBlackboard>();

            Idle idle = new Idle(this, _blackboard);
            Walking walking = new Walking(this, _blackboard);
            Running running = new Running(this, _blackboard);
            Falling falling = new Falling(this, _blackboard);
            CrouchIdle crouchIdle = new CrouchIdle(this, _blackboard);
            CrouchWalking crouchWalk = new CrouchWalking(this, _blackboard);

            At(falling, idle, IsGrounded);

            At(idle, walking, IsMoving);
            At(idle, crouchIdle, () => IsCrouching() && !IsMoving());

            At(walking, running, IsRunning);
            At(walking, crouchIdle, () => IsCrouching() && !IsMoving());
            At(walking, crouchWalk, () => IsCrouching() && IsMoving());

            At(crouchIdle, running, () => !IsCrouchingBlocked() && IsRunning());
            At(crouchIdle, walking, () => !IsCrouching() && IsMoving());
            At(crouchIdle, crouchWalk, () => IsCrouching() && IsMoving());

            At(crouchWalk, crouchIdle, () => IsCrouching() && !IsMoving());
            At(crouchWalk, running, () => !IsCrouchingBlocked() && IsRunning());
            At(crouchWalk, walking, () => !IsCrouching() && IsMoving());

            At(running, walking, () => !IsRunning());

            AtAny(falling, () => !IsGrounded());
            AtAny(idle, () => !IsMoving() && IsGrounded() && !IsCrouching());

            void At(IState from, IState to, Func<bool> condition)
            {
                _stateMachine.AddTransition(from, to, condition);
            }

            void AtAny(IState to, Func<bool> condition)
            {
                _stateMachine.AddAnyTransition(to, condition);
            }

            _stateMachine.SetState(idle);
        }

        private void Update()
        {
            _stateMachine.Tick();
            _blackboard.currentPlayerState = _stateMachine.GetCurrentStateType();
        }

        public PlayerState GetCurrentStateType()
        {
            return _stateMachine.GetCurrentStateType();
        }

        public PlayerState GetPreviousStateType()
        {
            return _stateMachine.GetPreviousStateType();
        }

        private bool IsGrounded()
        {
            return _blackboard.IsGrounded;
        }

        private bool IsMoving()
        {
            return _blackboard.IsMoving;
        }

        private bool IsRunning()
        {
            return _blackboard.IsRunning && _blackboard.IsMovingForward;
        }

        private bool IsCrouching()
        {
            return _blackboard.IsCrouching || _blackboard.IsCrouchingBlocked;
        }

        private bool IsCrouchingBlocked()
        {
            return _blackboard.IsCrouchingBlocked;
        }
    }
}