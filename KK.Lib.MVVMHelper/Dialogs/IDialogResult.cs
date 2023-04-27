namespace KK.Lib.MVVMHelper.Dialogs;

public interface IDialogResult
{
    IDialogParameters Parameters { get; }

    ButtonResults Result { get; }
}
