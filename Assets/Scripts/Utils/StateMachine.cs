using System.Collections.Generic;
using ILogger = GameLog.ILogger;

public class StateMachine<T>
{
    public delegate void StateFunc();

    protected State m_currentState;
    protected Dictionary<T, State> m_states;
    protected ILogger m_logger;

    public T currentState => m_currentState.Id;

    public StateMachine()
    {
        m_currentState = null;
        m_states = new Dictionary<T, State>();
        m_logger = null;
    }

    public StateMachine(ILogger logger)
    {
        m_currentState = null;
        m_states = new Dictionary<T, State>();
        m_logger = logger;
    }

    public void Add(T id, StateFunc enter, StateFunc update, StateFunc leave)
    {
        m_states.Add(id, new State(id, enter, update, leave));
    }

    public void Update()
    {
        if (m_currentState == null) return;
        if (m_currentState.Update == null) return;

        m_currentState.Update();
    }

    public void Shutdown()
    {
        if (m_currentState != null && m_currentState.Leave != null)
            m_currentState.Leave();

        m_currentState = null;
    }

    public void SwitchTo(T state)
    {
        var newState = m_states[state];

        if (m_currentState != null && m_currentState.Leave != null)
            m_currentState.Leave();

        // no need to check if new state is valid, 
        // dictionary throws exception if key not found
        if (newState.Enter != null)
            newState.Enter();

        m_currentState = newState;
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