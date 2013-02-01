using System;
using System.Reflection;

public class StateBuilderUtility : IStateBuilder
{

	private StateGraphBuilder _stateGraphBuilder;

	private IStateParameterFactory _parameterFactory;
	public StateBuilderUtility(StateGraphBuilder stateGraphBuilder, IStateParameterFactory parameterFactory)
	{
		this._stateGraphBuilder = stateGraphBuilder;
		this._parameterFactory = parameterFactory;
	}

	private ConstructorInfo GetBuildConstructor(Type type)
	{
		ConstructorInfo attributedConstructor = null;
		ConstructorInfo longestConstructor = null;
		int longestArgumentCount = -1;
		bool multipleAtLongestFound = false;
		foreach (ConstructorInfo c in type.GetConstructors()) {
			if (Attribute.IsDefined(c, typeof(StateConstructorAttribute))) {
				if ((attributedConstructor != null)) {
					throw new InvalidOperationException(string.Format("Type {0} has multiple constructors marked with the StateConstructorAttribute. Either remove one or both.", type));
				}
				attributedConstructor = c;
			}
			if (c.GetParameters().Length > longestArgumentCount) {
				longestArgumentCount = c.GetParameters().Length;
				longestConstructor = c;
				multipleAtLongestFound = false;
			} else if (c.GetParameters().Length == longestArgumentCount) {
				multipleAtLongestFound = true;
			}
		}
		if ((attributedConstructor != null)) {
			return attributedConstructor;
		} else if (multipleAtLongestFound) {
			throw new InvalidOperationException(string.Format("Couldn't decide which constructor to use to build type {0}. Multiple constructors requiring {1} parameters were found, and no constructor was marked with the StateConstructor attribute. Mark the desired constructor with the attribute, or ensure only one constructor exists with the longest parameter count", type, longestArgumentCount));
		} else if ((longestConstructor != null)) {
			return longestConstructor;
		} else {
			throw new InvalidOperationException(string.Format("Couldn't find any constructors for type {0}", type));
		}
	}

	private StateGraph<TState, TCommonInterface> GetStateGraph<TState, TCommonInterface>() where TState : struct where TCommonInterface : class
	{
		return this._stateGraphBuilder.BuildGraph<TState, TCommonInterface>();
	}

	public object Build(System.Type type)
	{
		if (type.IsGenericType && typeof(StateGraph<, >).Equals(type.GetGenericTypeDefinition())) {
			Type[] genericArguments = type.GetGenericArguments();
			MethodInfo buildMethod = this.GetType().GetMethod("GetStateGraph").MakeGenericMethod(genericArguments);
			return buildMethod.Invoke(this, null);
		}
		ConstructorInfo constructor = this.GetBuildConstructor(type);
		ParameterInfo[] requiredParameters = constructor.GetParameters();
		object[] paramList = new object[requiredParameters.Length];
		for (int paramIndex = 0; paramIndex <= paramList.Length - 1; paramIndex++) {
			ParameterInfo paramInfo = requiredParameters[paramIndex];
			paramList[paramIndex] = this._parameterFactory.GetParameterValue(paramInfo, this);
		}
		return constructor.Invoke(paramList);
	}
}
