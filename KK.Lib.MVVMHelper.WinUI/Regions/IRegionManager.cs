using Microsoft.UI.Xaml;

namespace KK.Lib.MVVMHelper.Regions;

public interface IRegionManager
{
    void RegisterForNavigation<TView>(string viewName)
        where TView : FrameworkElement;

    void RequestNavigate(string regionName, string viewName);
}
