namespace KK.Lib.MVVMHelper;

public interface IWireViewModel<TViewModel>
    where TViewModel : class
{
    public TViewModel? ViewModel { get; set; }
}
