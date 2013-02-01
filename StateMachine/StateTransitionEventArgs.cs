using System;

public class StateTransitionEventArgs : EventArgs
{

	private Enum _fromState;
	private Enum _toState;

	private bool _reinvokeInput;
	public StateTransitionEventArgs(Enum fromState, Enum toState, bool reinvokeInput)
	{
		this._fromState = fromState;
		this._toState = toState;
		this._reinvokeInput = reinvokeInput;
	}

	public virtual Enum FromState {
		get { return this._fromState; }
	}

	public virtual Enum ToState {
		get { return this._toState; }
	}

	public virtual bool ReinvokeInput {
		get { return this._reinvokeInput; }
	}

}
