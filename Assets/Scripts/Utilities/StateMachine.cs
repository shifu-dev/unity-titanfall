using System.Collections.Generic;

public class StateMachine<T>
{
    public delegate void StateFunc();

    protected State _curState;
    protected Dictionary<T, State> _states;

    public T currentState => _curState.Id;

    public StateMachine()
    {
        _curState = null;
        _states = new Dictionary<T, State>();
    }

    public void Add(T id, StateFunc enter, StateFunc update, StateFunc leave)
    {
        _states.Add(id, new State(id, enter, update, leave));
    }

    public void Update()
    {
        if (_curState is null) return;
        if (_curState.Update is null) return;

        _curState.Update();
    }

    public void Shutdown()
    {
        if (_curState is not null && _curState.Leave is not null)
            _curState.Leave();

        _curState = null;
    }

    public void Switch(T state)
    {
        var newState = _states[state];

        if (_curState is not null && _curState.Leave is not null)
            _curState.Leave();

        // no need to check if new state is valid, 
        // dictionary throws exception if key not found
        if (newState.Enter is not null)
            newState.Enter();

        _curState = newState;
    }

    protected class State
    {
        public State(T id, StateFunc enter, StateFunc update, StateFunc leave)
        {
            Id = id;
            Enter = enter;
            Update = update;
            Leave = leave;
        }

        public T Id;
        public StateFunc Enter;
        public StateFunc Update;
        public StateFunc Leave;
    }
}