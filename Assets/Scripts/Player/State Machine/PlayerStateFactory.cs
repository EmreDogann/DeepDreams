using System.Collections.Generic;

namespace DeepDreams.Player.State_Machine
{
    public enum PlayerState
    {
        Idle,
        Walk,
        Run,
        Crouch,
        Grounded,
        Fall
    }

    public class PlayerStateFactory
    {
        readonly PlayerStateMachine _context;
        readonly Dictionary<PlayerState, PlayerBaseState> _states;

        public PlayerStateFactory(PlayerStateMachine context) {
            _context = context;
            _states = new Dictionary<PlayerState, PlayerBaseState>
            {
                { PlayerState.Idle, new PlayerIdleState(_context, this) },
                { PlayerState.Walk, new PlayerWalkState(_context, this) },
                { PlayerState.Run, new PlayerRunState(_context, this) },
                { PlayerState.Crouch, new PlayerCrouchState(_context, this) },
                { PlayerState.Grounded, new PlayerGroundedState(_context, this) },
                { PlayerState.Fall, new PlayerFallState(_context, this) }
            };
        }

        public PlayerBaseState Get(PlayerState playerState) {
            return _states[playerState];
        }
    }
}