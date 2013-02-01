using System;

public interface ITransition
{


	event EventHandler<StateTransitionEventArgs> StateTransition;
}
