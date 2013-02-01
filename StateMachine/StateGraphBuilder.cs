using System;
using System.Reflection;
using System.Linq;

public class StateGraphBuilder
{


	private IStateParameterFactory _stateFactory;
	public StateGraphBuilder(IStateParameterFactory stateFactory)
	{
		this._stateFactory = stateFactory;
	}

	public StateGraph<TState, TCommonInterface> BuildGraph<TState, TCommonInterface>() where TState : struct where TCommonInterface : class
	{
		Type enumType = typeof(TState).UnderlyingSystemType;
		if (!enumType.IsEnum) {
			throw new ArgumentException("TState must be an enumeration of the states for this graph");
		}

		StateGraph<TState, TCommonInterface> sg = null;

        FieldInfo defaultField = (from f in enumType.GetFields() where Attribute.IsDefined(f, typeof(DefaultStateAttribute)) select f).SingleOrDefault();

		if ((defaultField != null)) {
			sg = this.GetStateGraph<TState, TCommonInterface>(defaultField, enumType);
		}
		foreach (FieldInfo f in enumType.GetFields()) {
			if (!f.IsSpecialName) {
				if (sg == null) {
					sg = this.GetStateGraph<TState, TCommonInterface>(f, enumType);
				} else {
					TState state = (TState)f.GetValue(enumType);
					TCommonInterface handler = (TCommonInterface) this.GetHandler(f);
					sg.AddNode(state, handler);
				}
			}
		}
		return sg;
	}

	private object GetHandler(MemberInfo memberInfo)
	{
		Type handlerType = ((StateHandlerAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(StateHandlerAttribute))).Handler;
		return new StateBuilderUtility(this, this._stateFactory).Build(handlerType);
	}

	private StateGraph<TState, TCommonInterface> GetStateGraph<TState, TCommonInterface>(FieldInfo field, Type enumType) where TState : struct where TCommonInterface : class
	{
		TCommonInterface defaultHandler = (TCommonInterface) this.GetHandler(field);
		TState defaultState = (TState)field.GetValue(enumType);
		return new StateGraph<TState, TCommonInterface>(defaultState, defaultHandler);
	}

}
