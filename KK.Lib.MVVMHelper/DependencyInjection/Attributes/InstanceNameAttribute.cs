namespace KK.Lib.MVVMHelper.DependencyInjection.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class InstanceNameAttribute : Attribute
{
    internal string Name { get; }

    public InstanceNameAttribute(string name) => Name = name;
}
