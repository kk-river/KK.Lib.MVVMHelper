using System.Windows;
using System.Windows.Controls;
using KK.Lib.MVVMHelper.DependencyInjection;

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

    private readonly Dictionary<string, ViewInfo> _viewInfoList = new();

    internal RegionManager(IContainer container)
    {
        _container = container;
    }

    public void RegisterForNavigation<TView>(string viewName)
        where TView : FrameworkElement
    {
        RegisterForNavigationInternal(viewName, typeof(TView), null);
    }

    public void RegisterForNavigation<TView, TViewModel>(string viewName)
        where TView : FrameworkElement
        where TViewModel : class
    {
        RegisterForNavigationInternal(viewName, typeof(TView), typeof(TViewModel));
    }

    private void RegisterForNavigationInternal(string viewName, Type viewType, Type? viewModelType)
    {
        if (_viewInfoList.ContainsKey(viewName))
        {
            throw new ArgumentException($"Same viewName already registered. viewName = {viewName}.");
        }

        _viewInfoList.Add(viewName, new ViewInfo(viewType, viewModelType));
    }

    public void RequestNavigate(string regionName, string viewName)
    {
        if (!_viewInfoList.TryGetValue(viewName, out ViewInfo? viewInfo))
        {
            throw new NotImplementedException($"Unknown viewName. viewName = {viewName}.");
        }

        FrameworkElement view = (FrameworkElement)_container.InstantiateFromType(viewInfo.ViewType);

        //Attach ViewModel if exists.
        if (viewInfo.ViewModelType is not null)
        {
            object viewModel = _container.InstantiateFromType(viewInfo.ViewModelType);
            view.DataContext = viewModel;
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
                _ = p.Children.Add(elem);
                break;
            default:
                throw new NotImplementedException($"Unknown region type: {region.GetType()}.");
        }
    }

    private record ViewInfo(Type ViewType, Type? ViewModelType);
}
