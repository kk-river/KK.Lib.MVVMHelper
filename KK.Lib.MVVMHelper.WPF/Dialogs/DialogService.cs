using System.Windows;
using KK.Lib.MVVMHelper.DependencyInjection;

namespace KK.Lib.MVVMHelper.Dialogs;

public class DialogService : IDialogService
{
    private readonly IContainer _container;
    private readonly Dictionary<string, ViewInfo> _viewInfoList = new();
    private readonly Dictionary<string, Type> _windowTypes = new();

    internal DialogService(IContainer container)
    {
        _container = container;
    }

    public void RegisterDialog<TView, TViewModel>(string viewName)
        where TView : FrameworkElement
        where TViewModel : IDialogAware
    {
        Type viewType = typeof(TView);
        Type viewModelType = typeof(TViewModel);

        RegisterViewInternal(viewName, viewType, viewModelType);
    }

    private void RegisterViewInternal(string viewName, Type viewType, Type viewModelType)
    {
        if (_viewInfoList.ContainsKey(viewName))
        {
            throw new ArgumentException($"Same viewName already exists. viewName = {viewName}.");
        }

        _viewInfoList.Add(viewName, new ViewInfo(viewType, viewModelType));
    }

    public void RegisterWindow<TWindow>(string windowName) where TWindow : Window, new()
        => _windowTypes[windowName] = typeof(TWindow);

    public void Show(string viewName, IDialogParameters parameters, Action<IDialogResult> callback) => ShowDialogInternal(viewName, static () => new DialogWindow(), parameters, callback, false);
    public void Show(string viewName, string windowName, IDialogParameters parameters, Action<IDialogResult> callback) => ShowDialogInternal(viewName, () => (IDialogWindow)Activator.CreateInstance(_windowTypes[windowName])!, parameters, callback, false);

    public void ShowDialog(string viewName, IDialogParameters parameters, Action<IDialogResult> callback) => ShowDialogInternal(viewName, static () => new DialogWindow(), parameters, callback, true);
    public void ShowDialog(string viewName, string windowName, IDialogParameters parameters, Action<IDialogResult> callback) => ShowDialogInternal(viewName, () => (IDialogWindow)Activator.CreateInstance(_windowTypes[windowName])!, parameters, callback, true);

    private void ShowDialogInternal(string viewName, Func<IDialogWindow> lazyWindow, IDialogParameters parameters, Action<IDialogResult> callback, bool isModel)
    {
        if (!_viewInfoList.TryGetValue(viewName, out ViewInfo? viewInfo))
        {
            throw new NotImplementedException($"Unknown viewName. viewName = {viewName}.");
        }

        //Create view and viewModel
        FrameworkElement view = (FrameworkElement)_container.InstantiateFromType(viewInfo.ViewType);
        object viewModel = _container.InstantiateFromType(viewInfo.ViewModelType);

        //Create dialog host.
        IDialogWindow dialogHost = lazyWindow();
        dialogHost.Content = view;
        dialogHost.DataContext = viewModel;
        dialogHost.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive)!;

        ConfigureWindowEvents(dialogHost, parameters, callback);

        if (isModel)
        {
            _ = dialogHost.ShowDialog();
        }
        else
        {
            dialogHost.Show();
        }
    }

    private static void ConfigureWindowEvents(IDialogWindow dialogHost, IDialogParameters parameters, Action<IDialogResult> callback)
    {
        IDialogAware dialogAware = (IDialogAware)dialogHost.DataContext;
        IDialogResult? result = null;

        void DialogHost_Loaded(object sender, RoutedEventArgs e)
        {
            dialogAware.OnDialogOpened(parameters);
        }

        void DialogHost_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!dialogAware.CanCloseDialog())
            {
                e.Cancel = true;
            }
        }

        void DialogHost_Closed(object? sender, EventArgs e)
        {
            dialogHost.Loaded -= DialogHost_Loaded;
            dialogHost.Closing -= DialogHost_Closing;
            dialogHost.Closed -= DialogHost_Closed;
            dialogAware.RequestClose -= DialogAware_RequestClose;

            dialogAware.OnDialogClosed();
            result ??= new DialogResult(ButtonResults.None);

            callback(result);

            dialogHost.Content = null!;
            dialogHost.DataContext = null!;
        }

        void DialogAware_RequestClose(IDialogResult obj)
        {
            result = obj;
            dialogHost.Close();
        }

        dialogHost.Loaded += DialogHost_Loaded;
        dialogHost.Closing += DialogHost_Closing;
        dialogHost.Closed += DialogHost_Closed;
        dialogAware.RequestClose += DialogAware_RequestClose;
    }

    private record ViewInfo(Type ViewType, Type ViewModelType);
}
