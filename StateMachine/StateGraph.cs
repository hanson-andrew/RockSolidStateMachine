using System;
using System.Collections.Generic;

public class StateGraph<TState, TCommonInterface>
    where TState : struct
    where TCommonInterface : class
{

    private IDictionary<TState, TCommonInterface> _nodeLookup = new Dictionary<TState, TCommonInterface>();
    private TState? _defaultState;
    private TCommonInterface _defaultHandler;
    private TState? _currentState;
    private TCommonInterface _currentHandler;

    private Action<TCommonInterface> _lastActionInvoked;
    public event EventHandler<StateGraphFinalizedEventArgs> StateGraphFinalized;

    public virtual TState? CurrentState
    {
        get { return this._currentState; }
    }

    public virtual TCommonInterface CurrentHandler
    {
        get { return this._currentHandler; }
    }

    public StateGraph(TState defaultState, TCommonInterface defaultHandler)
    {
        this._defaultState = defaultState;
        this._defaultHandler = defaultHandler;
        if (_defaultHandler is ITransition)
        {
            ((ITransition)_defaultHandler).StateTransition += this.HandleStateTransitioned;
        }
        this.Reinitialize();
    }

    public void Input(Action<TCommonInterface> invokeAction)
    {
        this._lastActionInvoked = invokeAction;
        invokeAction.Invoke(this.CurrentHandler);
    }

    public void AddNode(TState state, TCommonInterface handler)
    {
        if (this._nodeLookup.ContainsKey(state))
        {
            throw new ArgumentException("A previous state handler was already registered for state {0}", state.ToString());
        }
        this._nodeLookup.Add(state, handler);
        if (handler is ITransition)
        {
            ((ITransition)handler).StateTransition += this.HandleStateTransitioned;
        }
    }

    public void Reinitialize()
    {
        this._currentState = this._defaultState;
        this._currentHandler = this._defaultHandler;
    }

    private void HandleStateTransitioned(object sender, StateTransitionEventArgs e)
    {
        if (!(e.ToState is TState))
        {
            throw new ArgumentException(string.Format("Invalid object specified in ToState: {0}", e.ToState.GetType().FullName));
        }

        TState newState = (TState)(object)e.ToState;

        if (!this._nodeLookup.ContainsKey(newState))
        {
            throw new InvalidOperationException(string.Format("No handler was registerd for state {0}", newState.ToString()));
        }
        this._currentState = newState;
        this._currentHandler = this._nodeLookup[this._currentState.Value];

        if (e.ReinvokeInput)
        {
            this._lastActionInvoked.Invoke(this.CurrentHandler);
        }

        if (!(this.CurrentHandler is ITransition))
        {
            if (StateGraphFinalized != null)
            {
                StateGraphFinalized(this, new StateGraphFinalizedEventArgs());
            }
        }
    }

}
