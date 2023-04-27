using KK.Lib.MVVMHelper.DependencyInjection;
using Microsoft.UI.Xaml;
using KK.Lib.MVVMHelper.Regions;
using KK.Lib.MVVMHelper.Dialogs;
using System.Diagnostics.CodeAnalysis;

namespace KK.Lib.MVVMHelper;

public abstract class MVVMApplication : Application
{
    private RegionManager? _regionManager;
    private DialogService? _dialogService;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        IContainer container = new ReflectionContainer();

        ConfigureServicesInternal(container);
        ConfigureServices(container);
        ConfigureViews(_regionManager, _dialogService);

        _dialogService.ShowWindow(GetWindowType());
    }

    [MemberNotNull(nameof(_regionManager), nameof(_dialogService))]
    private void ConfigureServicesInternal(IContainer container)
    {
        container.RegisterInstance<IRegionManager>(_regionManager = new RegionManager(container));
        container.RegisterInstance<IDialogService>(_dialogService = new DialogService(container));
    }

    public abstract Type GetWindowType();

    public abstract void ConfigureServices(IContainer container);

    public virtual void ConfigureViews(IRegionManager regionManager, IDialogService dialogService) { }
}
