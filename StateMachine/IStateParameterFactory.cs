using System.Reflection;

public interface IStateParameterFactory
{

	object GetParameterValue(ParameterInfo parameterInfo, IStateBuilder defaultBuilder);

}
