namespace KK.Lib.MVVMHelper.Dialogs;

public static class IDialogServiceExtensions
{
    public static void Show(this IDialogService dialogService, string viewName)
        => dialogService.Show(viewName, new DialogParameters(), _ => { });

    public static void Show(this IDialogService dialogService, string viewName, Action<IDialogResult> callback)
        => dialogService.Show(viewName, new DialogParameters(), callback);

    public static void Show(this IDialogService dialogService, string viewName, string windowName)
        => dialogService.Show(viewName, windowName, new DialogParameters(), _ => { });

    public static void Show(this IDialogService dialogService, string viewName, string windowName, Action<IDialogResult> callback)
        => dialogService.Show(viewName, windowName, new DialogParameters(), callback);

    public static void ShowDialog(this IDialogService dialogService, string viewName)
        => dialogService.ShowDialog(viewName, new DialogParameters(), _ => { });

    public static void ShowDialog(this IDialogService dialogService, string viewName, Action<IDialogResult> callback)
        => dialogService.ShowDialog(viewName, new DialogParameters(), callback);

    public static void ShowDialog(this IDialogService dialogService, string viewName, string windowName)
        => dialogService.ShowDialog(viewName, windowName, new DialogParameters(), _ => { });

    public static void ShowDialog(this IDialogService dialogService, string viewName, string windowName, Action<IDialogResult> callback)
        => dialogService.ShowDialog(viewName, windowName, new DialogParameters(), callback);
}
