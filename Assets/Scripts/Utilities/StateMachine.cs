using System.Collections.Generic;
using GameFramework.Logging;

public class StateMachine<T>
{
    public delegate void StateFunc();

    protected State m_curState;
    protected Dictionary<T, State> m_states;
    protected IGameLogger m_logger;

    public T currentState => m_curState.Id;

    public StateMachine()
    {
        m_curState = null;
        m_states = new Dictionary<T, State>();
        m_logger = null;
    }

    public StateMachine(IGameLogger logger)
    {
        m_curState = null;
        m_states = new Dictionary<T, State>();
        m_logger = logger;
    }

    public void Add(T id, StateFunc enter, StateFunc update, StateFunc leave)
    {
        m_states.Add(id, new State(id, enter, update, leave));
    }

    public void Update()
    {
        if (m_curState is null) return;
        if (m_curState.Update is null) return;

        m_curState.Update();
    }

    public void Shutdown()
    {
        if (m_curState is not null && m_curState.Leave is not null)
            m_curState.Leave();

        m_curState = null;
    }

    public void Switch(T state)
    {
        var newState = m_states[state];

        if (m_curState is not null && m_curState.Leave is not null)
            m_curState.Leave();

        // no need to check if new state is valid, 
        // dictionary throws exception if key not found
        if (newState.Enter is not null)
            newState.Enter();

        m_curState = newState;
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