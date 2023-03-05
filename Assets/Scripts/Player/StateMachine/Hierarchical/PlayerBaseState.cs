namespace DeepDreams.Player.StateMachine.Hierarchical
{
    public abstract class PlayerBaseState
    {
        private bool _isRootState;
        private PlayerState _stateType;
        private PlayerBaseState _currentSuperState;
        private PlayerBaseState _currentSubState;

        protected bool IsRootState
        {
            set => _isRootState = value;
        }

        protected PlayerHierarchicalStateMachine Ctx { get; }
        protected PlayerStateFactory Factory { get; }

        public PlayerBaseState(PlayerHierarchicalStateMachine currentContext, PlayerStateFactory stateFactory, PlayerState playerState)
        {
            Ctx = currentContext;
            Factory = stateFactory;
            _stateType = playerState;

            // Checks if the instance of this abstract class implements the IRootState interface.
            _isRootState = this is IRootState;
        }

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
        public abstract bool CheckSwitchState();
        public abstract void InitializeSubState();

        public void UpdateStates()
        {
            UpdateState();
            if (_currentSubState != null) _currentSubState.UpdateStates();
        }

        public void ExitStates()
        {
            ExitState();
            if (_currentSubState != null) _currentSubState.ExitStates();
        }

        protected void SwitchState(PlayerBaseState newState)
        {
            ExitState();
            newState.EnterState();

            if (_isRootState)
            {
                // CurrentState is the root state, so only roots can be assigned to this.
                Ctx.CurrentState = newState;
            }
            else if (_currentSuperState != null)
            {
                // If the new state to switch to doesn't already have a parent, set this node to its parent.
                _currentSuperState.SetSubState(newState);
            }
        }

        protected void SetSuperState(PlayerBaseState newSuperState)
        {
            _currentSuperState = newSuperState;
        }

        protected void SetSubState(PlayerBaseState newSubState)
        {
            _currentSubState = newSubState;
            newSubState.SetSuperState(this);
        }
    }
}