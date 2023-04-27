using System.Windows;

namespace KK.Lib.MVVMHelper.Dialogs;

public interface IDialogService
{
    void RegisterDialog<TView, TViewModel>(string viewName)
        where TView : FrameworkElement
        where TViewModel : IDialogAware;

    void Show(string viewName, IDialogParameters parameters, Action<IDialogResult> callback);
    void Show(string viewName, string windowName, IDialogParameters parameters, Action<IDialogResult> callback);

    void ShowDialog(string viewName, IDialogParameters parameters, Action<IDialogResult> callback);
    void ShowDialog(string viewName, string windowName, IDialogParameters parameters, Action<IDialogResult> callback);

    void RegisterWindow<TWindow>(string windowName)
        where TWindow : Window, new();
}
