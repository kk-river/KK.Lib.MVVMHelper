using System.Windows;

namespace KK.Lib.MVVMHelper.Regions;

public interface IRegionManager
{
    void RegisterForNavigation<TView>(string viewName)
        where TView : FrameworkElement;

    void RegisterForNavigation<TView, TViewModel>(string viewName)
        where TView : FrameworkElement
        where TViewModel : class;

    void RequestNavigate(string regionName, string viewName);
}
