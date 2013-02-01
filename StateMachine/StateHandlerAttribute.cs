using System;

[AttributeUsage(AttributeTargets.Field)]
public sealed class StateHandlerAttribute : Attribute
{


	public Type _handler;
	public StateHandlerAttribute(Type handler)
	{
		this._handler = handler;
	}

	public Type Handler {
		get { return this._handler; }
	}

}
