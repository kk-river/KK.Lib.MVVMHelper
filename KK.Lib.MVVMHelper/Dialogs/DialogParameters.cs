namespace KK.Lib.MVVMHelper.Dialogs;

public class DialogParameters : IDialogParameters
{
    private readonly Dictionary<string, object> _params = new();

    public void Add(string key, object value) => _params.Add(key, value);

    public T Get<T>(string key)
    {
        if (!_params.TryGetValue(key, out object? value))
        {
            throw new KeyNotFoundException($"Requested key does not existes. Key = {key}.");
        }

        if (value is not T casted)
        {
            throw new InvalidCastException($"Unable to convert the value of Type '{value.GetType().FullName}' to '{typeof(T).FullName}' for the key '{key}' ");
        }

        return casted;
    }

    public bool ContainsKey(string key) => _params.ContainsKey(key);
}
