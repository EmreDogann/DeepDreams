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
        Crouching
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
            Crouching crouching = new Crouching(this, _blackboard);

            At(falling, idle, IsGrounded);
            At(idle, walking, IsMoving);
            At(idle, crouching, IsCrouching);
            At(walking, running, IsRunning);
            At(walking, crouching, IsCrouching);
            At(crouching, running, () => !IsCrouchingBlocked() && IsRunning());
            At(crouching, walking, () => !IsCrouching());
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
            _blackboard.currentPlayerState = _stateMachine.GetStateType();
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