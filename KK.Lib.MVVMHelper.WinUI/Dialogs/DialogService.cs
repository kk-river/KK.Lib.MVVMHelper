using System.Reflection;
using KK.Lib.MVVMHelper.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace KK.Lib.MVVMHelper.Dialogs;

internal class DialogService : IDialogService
{
    private readonly IContainer _container;
    private readonly Dictionary<string, Func<ContentDialog>> _dialogWindowGenerators = new();
    private readonly Dictionary<string, ViewInfo> _viewInfoList = new();

    private static readonly List<Window> s_activatedQueue = new();
    private readonly Dictionary<string, Type> _windowTypes = new();
    public static Window? LastActivated { get => s_activatedQueue.Count == 0 ? null : s_activatedQueue[^1]; }

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

        if (!_viewInfoList.TryAdd(viewName, new ViewInfo(viewType, viewModelType)))
        {
            throw new ArgumentException($"Same viewName already exists. viewName = {viewName}.");
        }
    }

    public void RegisterDialogWindow<TDialogWindow>(string windowName)
        where TDialogWindow : ContentDialog, new()
    {
        _dialogWindowGenerators[windowName] = () => new TDialogWindow();
    }

    public void ShowDialog(string viewName, IDialogParameters parameters, Action<IDialogResult> callback)
        => ShowDialogInternal(viewName, null, parameters, callback);
    public void ShowDialog(string viewName, string windowName, IDialogParameters parameters, Action<IDialogResult> callback)
        => ShowDialogInternal(viewName, windowName, parameters, callback);

    private void ShowDialogInternal(string viewName, string? windowName, IDialogParameters parameters, Action<IDialogResult> callback)
    {
        if (!_viewInfoList.TryGetValue(viewName, out ViewInfo? viewInfo))
        {
            throw new NotImplementedException($"Unknown viewName. viewName = {viewName}.");
        }

        //Create view and viewModel
        FrameworkElement view = (FrameworkElement)_container.InstantiateFromType(viewInfo.ViewType);
        IDialogAware dialogAware = (IDialogAware)_container.InstantiateFromType(viewInfo.DialogAwareType);

        Type? viewModelType = GetViewModelTypeIfExists(viewInfo.ViewType);
        if (viewModelType is not null)
        {
            SetViewModelToView(view, dialogAware, viewInfo.ViewType, viewModelType);
        }

        //Create dialog host.
        ContentDialog dialogHost = windowName is not null ? _dialogWindowGenerators[windowName]() : new DialogWindow();
        dialogHost.Content = view;
        dialogHost.XamlRoot = LastActivated!.Content.XamlRoot;

        ConfigureWindowEvents(dialogHost, dialogAware, parameters, callback);

        _ = dialogHost.ShowAsync();
    }

    private static void ConfigureWindowEvents(ContentDialog dialogHost, IDialogAware dialogAware, IDialogParameters parameters, Action<IDialogResult> callback)
    {
        IDialogResult? result = null;

        void DialogHost_Loaded(object sender, RoutedEventArgs e)
        {
            dialogAware.OnDialogOpened(parameters);
            dialogHost.Unloaded += DialogHost_Unloaded; //ここで入れないとOpenの瞬間にUnloadが飛ぶ（なぜ。。。）
        }

        void DialogHost_Closing(object? sender, ContentDialogClosingEventArgs e)
        {
            if (!dialogAware.CanCloseDialog())
            {
                e.Cancel = true;
            }
        }

        void DialogHost_Closed(object? sender, ContentDialogClosedEventArgs e)
        {
            OnClosed(e.Result switch
            {
                ContentDialogResult.None => ButtonResults.None,
                ContentDialogResult.Primary => ButtonResults.Primary,
                ContentDialogResult.Secondary => ButtonResults.Secondary,
                _ => throw new InvalidProgramException(),
            });
        }

        void DialogHost_Unloaded(object sender, RoutedEventArgs e)
        {
            //親ウィンドウごと閉じた場合Closedはコールされないので
            if (dialogHost.Content is null) { return; }

            OnClosed(ButtonResults.None);
        }

        void DialogAware_RequestClose(IDialogResult obj)
        {
            result = obj;
            dialogHost.Hide();
        }

        void OnClosed(ButtonResults buttonResultAlt)
        {
            dialogHost.Loaded -= DialogHost_Loaded;
            dialogHost.Unloaded -= DialogHost_Unloaded;
            dialogHost.Closing -= DialogHost_Closing;
            dialogHost.Closed -= DialogHost_Closed;
            dialogAware.RequestClose -= DialogAware_RequestClose;

            dialogAware.OnDialogClosed();

            result ??= new DialogResult(buttonResultAlt);

            callback(result);

            dialogHost.Content = null!;
        }

        dialogHost.Loaded += DialogHost_Loaded;
        dialogHost.Closing += DialogHost_Closing;
        dialogHost.Closed += DialogHost_Closed;
        dialogAware.RequestClose += DialogAware_RequestClose;
    }

    public void RegisterWindow<TWindow>(string windowName)
        where TWindow : Window, new()
    {
        if (!_windowTypes.TryAdd(windowName, typeof(TWindow)))
        {
            throw new ArgumentException($"Same window already exists. windowName = {windowName}.");
        }
    }

    public void ShowWindow(string windowName)
    {
        if (!_windowTypes.TryGetValue(windowName, out Type? windowType))
        {
            throw new NotImplementedException($"Unknown viewName. viewName = {windowName}.");
        }

        ShowWindow(windowType);
    }

    public void ShowWindow(Type windowType)
    {
        Window newWindow = (Window)_container.InstantiateFromType(windowType);

        Type? viewModelType = GetViewModelTypeIfExists(windowType);
        if (viewModelType is not null)
        {
            object viewModel = _container.InstantiateFromType(viewModelType);

            SetViewModelToView(newWindow, viewModel, windowType, viewModelType);
        }

        TrackWindow(newWindow);

        newWindow.Activate();
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

    private static void TrackWindow(Window window)
    {
        window.Closed += (sender, args) =>
        {
            _ = s_activatedQueue.Remove(window);
        };

        window.Activated += (s, e) =>
        {
            _ = s_activatedQueue.Remove(window);
            s_activatedQueue.Add(window);
        };
    }

    private record ViewInfo(Type ViewType, Type DialogAwareType);
}
