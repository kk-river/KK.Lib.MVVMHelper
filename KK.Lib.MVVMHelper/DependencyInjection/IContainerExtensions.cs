namespace KK.Lib.MVVMHelper.DependencyInjection;

public static class IContainerExtensions
{
    public static void RegisterInstance<T>(this IContainer container, T instance) where T : class
        => container.RegisterInstance(typeof(T), instance);

    public static void RegisterInstance<T>(this IContainer container, T instance, string name) where T : class
        => container.RegisterInstance(typeof(T), instance, name);

    public static void RegisterSingleton<T>(this IContainer container)
        where T : class
        => container.RegisterSingleton(typeof(T), typeof(T));

    public static void RegisterSingleton<TFrom, TTo>(this IContainer container)
        where TFrom : class
        where TTo : TFrom
        => container.RegisterSingleton(typeof(TFrom), typeof(TTo));

    public static void RegisterSingleton<TFrom, TTo>(this IContainer container, string name)
        where TFrom : class
        where TTo : TFrom
        => container.RegisterSingleton(typeof(TFrom), typeof(TTo), name);

    public static T Resolve<T>(this IContainer container) where T : class
        => (T)container.Resolve(typeof(T));

    public static T Resolve<T>(this IContainer container, string name) where T : class
        => (T)container.Resolve(typeof(T), name);
}
