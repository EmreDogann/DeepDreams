using System;
using System.Collections.Generic;
using UnityEngine;

// From: https://www.youtube.com/watch?v=V75hgcsCGOM

namespace DeepDreams.Player.StateMachine.Simple
{
    public class StateMachine
    {
        private IState _currentState;
        private IState _prevState;

        private readonly Dictionary<Type, List<Transition>> _transitions = new Dictionary<Type, List<Transition>>();
        private List<Transition> _currentTransitions = new List<Transition>();
        private readonly List<Transition> _anyTransitions = new List<Transition>();

        private static readonly List<Transition> EmptyTransitions = new List<Transition>(0);

        public void Tick()
        {
            if (Time.timeScale == 0.0f) return;

            Transition transition = GetTransition();
            if (transition != null)
                SetState(transition.To);

            _currentState?.Tick();
        }

        public void SetState(IState state)
        {
            if (state == _currentState)
                return;

            _prevState = _currentState ?? state;
            _currentState = state;
            _prevState?.OnExit();

            _transitions.TryGetValue(_currentState.GetType(), out _currentTransitions);
            if (_currentTransitions == null)
                _currentTransitions = EmptyTransitions;

            _currentState.OnEnter();
        }

        public PlayerState GetCurrentStateType()
        {
            return _currentState.GetStateType();
        }

        public PlayerState GetPreviousStateType()
        {
            return _prevState.GetStateType();
        }

        public void AddTransition(IState from, IState to, Func<bool> predicate)
        {
            if (_transitions.TryGetValue(from.GetType(), out var transitions) == false)
            {
                transitions = new List<Transition>();
                _transitions[from.GetType()] = transitions;
            }

            transitions.Add(new Transition(to, predicate));
        }

        public void AddAnyTransition(IState state, Func<bool> predicate)
        {
            _anyTransitions.Add(new Transition(state, predicate));
        }

        private class Transition
        {
            public Func<bool> Condition { get; }
            public IState To { get; }

            public Transition(IState to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }
        }

        private Transition GetTransition()
        {
            foreach (Transition transition in _anyTransitions)
                if (transition.Condition())
                    return transition;

            foreach (Transition transition in _currentTransitions)
                if (transition.Condition())
                    return transition;

            return null;
        }
    }
}