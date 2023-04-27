namespace KK.Lib.MVVMHelper.DependencyInjection;

public interface IContainer
{
    void RegisterInstance(Type type, object instance);

    void RegisterInstance(Type type, object instance, string name);

    void RegisterSingleton(Type from, Type to);

    void RegisterSingleton(Type from, Type to, string name);

    object Resolve(Type type);

    object Resolve(Type type, string name);

    object InstantiateFromType(Type type);
}
