namespace KK.Lib.MVVMHelper.Dialogs;

public interface IDialogAware
{
    public string Title { get; }

    event Action<IDialogResult>? RequestClose;

    bool CanCloseDialog();

    void OnDialogOpened(IDialogParameters parameters);

    void OnDialogClosed();
}
