using System.Reflection;
using KK.Lib.MVVMHelper.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace KK.Lib.MVVMHelper.Regions;

public class RegionManager : IRegionManager
{
    private static readonly Dictionary<string, FrameworkElement> s_regions = new();

    #region Dependency
    public static readonly DependencyProperty RegionNameProperty = DependencyProperty.RegisterAttached("RegionName", typeof(string), typeof(RegionManager), new PropertyMetadata(defaultValue: null, OnSetRegionNameCallback));

    private static void OnSetRegionNameCallback(DependencyObject element, DependencyPropertyChangedEventArgs args) => s_regions.Add((string)args.NewValue, (FrameworkElement)element);

    public static void SetRegionName(DependencyObject regionTarget, string regionName) => regionTarget.SetValue(RegionNameProperty, regionName);

    public static string GetRegionName(DependencyObject regionTarget) => (string)regionTarget.GetValue(RegionNameProperty);
    #endregion Dependency

    private readonly IContainer _container;
    private readonly Dictionary<string, Type> _viewList = new();

    internal RegionManager(IContainer container)
    {
        _container = container;
    }

    public void RegisterForNavigation<TView>(string viewName)
        where TView : FrameworkElement
    {
        if (_viewList.ContainsKey(viewName))
        {
            throw new ArgumentException($"Same viewName already registered. viewName = {viewName}.");
        }

        _viewList.Add(viewName, typeof(TView));
    }

    public void RequestNavigate(string regionName, string viewName)
    {
        if (!_viewList.TryGetValue(viewName, out Type? viewType))
        {
            throw new NotImplementedException($"Unknown viewName. viewName = {viewName}.");
        }

        FrameworkElement view = (FrameworkElement)_container.InstantiateFromType(viewType);
        if (GetViewModelTypeIfExists(viewType) is Type viewModelType)
        {
            object viewModel = _container.InstantiateFromType(viewModelType);
            SetViewModelToView(view, viewModel, viewType, viewModelType);
        }

        FrameworkElement region = s_regions[regionName];
        switch (region)
        {
            case ContentControl cc:
                cc.Content = view;
                break;
            case ContentPresenter cp:
                cp.Content = view;
                break;
            case Panel p when view is UIElement elem:
                p.Children.Add(elem);
                break;
            default:
                throw new NotImplementedException($"Unknown region type: {region.GetType()}.");
        }
    }

    private static Type? GetViewModelTypeIfExists(Type type)
    {
        Type? viewModelType = type.GetInterface(typeof(IWireViewModel<>).FullName!)?.GenericTypeArguments[0];
        return viewModelType;
    }

    private static void SetViewModelToView(object view, object viewModel, Type viewType, Type viewModelType)
    {
        //ゴリ押し
        const string name = nameof(IWireViewModel<object>.ViewModel);
        if (viewType.GetProperty(name) is PropertyInfo propertyInfo)
        {
            propertyInfo.SetValue(view, viewModel);
        }
        else
        {
            Type viewTypeIWire = typeof(IWireViewModel<>).MakeGenericType(viewModelType);
            InterfaceMapping map = viewType.GetInterfaceMap(viewTypeIWire);
            MethodInfo methodInfo = map.InterfaceMethods.First(im => im.Name == $"set_{name}");

            _ = methodInfo.Invoke(view, new object[] { viewModel });
        }
    }
}
