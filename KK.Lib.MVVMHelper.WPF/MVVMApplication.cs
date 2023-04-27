using System.Diagnostics.CodeAnalysis;
using System.Windows;
using KK.Lib.MVVMHelper.DependencyInjection;
using KK.Lib.MVVMHelper.Dialogs;
using KK.Lib.MVVMHelper.Regions;

namespace KK.Lib.MVVMHelper;

public abstract class MVVMApplication : Application
{
    private RegionManager? _regionManager;
    private DialogService? _dialogService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        IContainer container = new ReflectionContainer();

        ConfigureServicesInternal(container);
        ConfigureServices(container);
        ConfigureViews(_regionManager, _dialogService);

        Window window = (Window)container.InstantiateFromType(GetWindowType());

        //Attach ViewModel if exists.
        if (GetWindowViewModelType() is Type windowViewModelType)
        {
            window.DataContext = container.InstantiateFromType(windowViewModelType);
        }

        window.Show();
    }

    [MemberNotNull(nameof(_regionManager), nameof(_dialogService))]
    private void ConfigureServicesInternal(IContainer container)
    {
        container.RegisterInstance<IRegionManager>(_regionManager = new RegionManager(container));
        container.RegisterInstance<IDialogService>(_dialogService = new DialogService(container));
    }

    public abstract Type GetWindowType();

    public virtual Type? GetWindowViewModelType() => null;

    public abstract void ConfigureServices(IContainer container);

    public virtual void ConfigureViews(IRegionManager regionManager, IDialogService dialogService) { }
}
