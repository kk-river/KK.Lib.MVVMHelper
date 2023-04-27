namespace KK.Lib.MVVMHelper.Dialogs;

public interface IDialogParameters
{
    public void Add(string key, object value);

    public T Get<T>(string key);

    public bool ContainsKey(string key);
}
