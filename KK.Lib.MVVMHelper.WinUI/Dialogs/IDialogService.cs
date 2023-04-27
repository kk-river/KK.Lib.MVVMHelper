using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace KK.Lib.MVVMHelper.Dialogs;

public interface IDialogService
{
    void RegisterDialog<TView, TViewModel>(string viewName)
        where TView : FrameworkElement
        where TViewModel : IDialogAware;

    void RegisterDialogWindow<TDialogWindow>(string windowName)
        where TDialogWindow : ContentDialog, new();

    void ShowDialog(string viewName, IDialogParameters parameters, Action<IDialogResult> callback);
    void ShowDialog(string viewName, string windowName, IDialogParameters parameters, Action<IDialogResult> callback);

    void RegisterWindow<TWindow>(string windowName)
        where TWindow : Window, new();

    void ShowWindow(string windowName);
    void ShowWindow(Type windowType);
}
