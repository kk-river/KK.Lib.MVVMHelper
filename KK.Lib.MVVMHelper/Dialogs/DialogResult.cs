namespace KK.Lib.MVVMHelper.Dialogs;

public record DialogResult(ButtonResults Result, IDialogParameters Parameters) : IDialogResult
{
    public DialogResult(ButtonResults result) : this(result, new DialogParameters()) { }
}
