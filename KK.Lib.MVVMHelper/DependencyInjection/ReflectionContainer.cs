using System.Reflection;
using KK.Lib.MVVMHelper.DependencyInjection.Attributes;

namespace KK.Lib.MVVMHelper.DependencyInjection;

public class ReflectionContainer : IContainer
{
    private readonly Dictionary<Type, Dictionary<string, object>> _created = new();
    private readonly Dictionary<Type, Dictionary<string, Type>> _notCreated = new();

    public ReflectionContainer()
    {
        RegisterInstance(typeof(IContainer), this);
    }

    public void RegisterInstance(Type type, object instance) => RegisterInstanceInternal(type, instance, "");
    public void RegisterInstance(Type type, object instance, string name)
    {
        if (string.IsNullOrEmpty(name)) { throw new ArgumentNullException(nameof(name), "Must not be null or empty."); }

        RegisterInstanceInternal(type, instance, name);
    }

    private void RegisterInstanceInternal(Type type, object instance, string name)
    {
        if (!_created.TryGetValue(type, out Dictionary<string, object>? createdObjects))
        {
            createdObjects = new();
            _created.Add(type, createdObjects);
        }

        createdObjects[name] = instance;
    }

    public void RegisterSingleton(Type tInterface, Type tImpl) => RegisterSingletonInternal(tInterface, tImpl, "");
    public void RegisterSingleton(Type tInterface, Type tImpl, string name)
    {
        if (string.IsNullOrEmpty(name)) { throw new ArgumentNullException(nameof(name), "Must not be null or empty."); }

        RegisterSingletonInternal(tInterface, tImpl, name);
    }

    private void RegisterSingletonInternal(Type tInterface, Type tImpl, string name)
    {
        if (!_notCreated.TryGetValue(tInterface, out Dictionary<string, Type>? notCreatedObjects))
        {
            notCreatedObjects = new();
            _notCreated.Add(tInterface, notCreatedObjects);
        }

        notCreatedObjects[name] = tImpl;
    }

    public object Resolve(Type tInterface) => Resolve(tInterface, "");
    public object Resolve(Type tInterface, string name)
    {
        //生成済みリストから名前で引けたらそれを返す
        if (_created.TryGetValue(tInterface, out Dictionary<string, object>? createdObjects)
            && createdObjects.TryGetValue(name, out object? o))
        {
            return o;
        }

        //未生成リストから名前で引けたら作る
        if (_notCreated.TryGetValue(tInterface, out Dictionary<string, Type>? notCreatedObjects)
            && notCreatedObjects.TryGetValue(name, out Type? tImpl))
        {
            object? created = InstantiateFromType(tImpl);
            if (created is not null)
            {
                _notCreated[tInterface].Remove(name);
                RegisterInstanceInternal(tInterface, created, name); //登録されたものなのでキャッシュする
                return created;
            }
        }

        //未登録でも生成できそうなら生成する
        if (tInterface.IsClass)
        {
            //こっちは勝手に生成しているのでキャッシュしない
            return InstantiateFromType(tInterface);
        }

        throw new KeyNotFoundException($"Designated type, {tInterface}{(string.IsNullOrEmpty(name) ? "" : $" named {name}")}, is not registered.");
    }

    public object InstantiateFromType(Type type)
    {
        ConstructorInfo[] constructorInfos = type.GetConstructors();
        if (!constructorInfos.Any())
        {
            throw new ContainerException($"Fail to instantiate. Target type has no constructor. Type = {type.FullName}");
        }

        ConstructorInfo constructorInfo = constructorInfos.FirstOrDefault(ci => ci.GetCustomAttribute<PreferentialAttribute>() is not null) ?? type.GetConstructors().First();

        try
        {
            //引数をコンテナから取得
            object[] parameters = constructorInfo.GetParameters()
                .Select(paramInfo => paramInfo.GetCustomAttribute(typeof(InstanceNameAttribute)) is InstanceNameAttribute attribute
                        ? Resolve(paramInfo.ParameterType, attribute.Name)
                        : Resolve(paramInfo.ParameterType))
                .ToArray();

            //生成
            return constructorInfo.Invoke(parameters);
        }
        catch (Exception ex)
        {
            throw new ContainerException($"Fail to instantiate by exception while invoking ctor. Type = {type.FullName}", ex);
        }
    }
}
